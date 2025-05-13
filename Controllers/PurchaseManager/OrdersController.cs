using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportComplexAPI.Data;
using SportComplexAPI.DTOs.PurchaseManager;
using SportComplexAPI.Models;

namespace SportComplexAPI.Controllers.PurchaseManager
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : Controller
    {
        private readonly SportComplexContext _context;

        public OrdersController(SportComplexContext context)
        {
            _context = context;
        }
        [HttpGet("all-orders")]
        public async Task<ActionResult<IEnumerable<OrderDto>>> GetOrders(
            [FromQuery] string? sortBy,
            [FromQuery] string? order,
            [FromQuery] int? supplierId,
            [FromQuery] int? brandId,
            [FromQuery] decimal? minTotal,
            [FromQuery] decimal? maxTotal,
            [FromQuery] DateTime? orderDate
        )
        {
            var query = _context.Orders
                .Include(o => o.Supplier)
                .Include(o => o.PaymentMethod)
                .Include(o => o.OrderStatus)
                .Include(o => o.PurchasedProducts)
                    .ThenInclude(pp => pp.Product)
                        .ThenInclude(p => p.Brand)
                .Include(o => o.PurchasedProducts)
                    .ThenInclude(pp => pp.Product)
                        .ThenInclude(p => p.ProductType)
                .Include(o => o.PurchasedProducts)
                    .ThenInclude(pp => pp.Deliveries)
                        .ThenInclude(d => d.Inventory)
                            .ThenInclude(i => i.Gym)
                .AsQueryable();

            // Фільтрація
            if (supplierId.HasValue && supplierId > 0)
                query = query.Where(o => o.Supplier.supplier_id == supplierId.Value);

            if (brandId.HasValue && brandId > 0)
                query = query.Where(o => o.PurchasedProducts.Any(pp => pp.Product.brand_id == brandId.Value));

            if (minTotal.HasValue)
                query = query.Where(o => o.order_total_price >= minTotal.Value);

            if (maxTotal.HasValue)
                query = query.Where(o => o.order_total_price <= maxTotal.Value);

            if (orderDate.HasValue)
                query = query.Where(o => o.order_date.Date == orderDate.Value.Date);

            // Сортування
            if (!string.IsNullOrEmpty(sortBy))
            {
                bool ascending = order == "asc";
                query = sortBy switch
                {
                    "orderNumber" => ascending ? query.OrderBy(o => o.order_number) : query.OrderByDescending(o => o.order_number),
                    "orderDate" => ascending ? query.OrderBy(o => o.order_date) : query.OrderByDescending(o => o.order_date),
                    _ => query.OrderByDescending(o => o.order_date) // default
                };
            }

            var orders = await query.ToListAsync();

            var result = orders.Select(o => new OrderDto
            {
                OrderId = o.order_id,
                OrderNumber = o.order_number,
                OrderDate = o.order_date,
                OrderTotalPrice = o.order_total_price,
                PaymentMethod = o.PaymentMethod.payment_method,
                OrderStatus = o.OrderStatus.order_status_name,
                SupplierName = o.Supplier.supplier_name,
                PurchasedProducts = o.PurchasedProducts.Select(pp => new PurchasedProductDto
                {
                    PurchasedProductId = pp.purchased_product_id,
                    ProductName = pp.Product.product_model,
                    Quantity = pp.quantity,
                    UnitPrice = pp.unit_price,
                    BrandName = pp.Product.Brand.brand_name,
                    ProductType = pp.Product.ProductType.product_type_name,
                    Deliveries = pp.Deliveries.Select(d => new DeliveryDto
                    {
                        DeliveryId = d.delivery_id,
                        DeliveryDate = d.delivery_date,
                        DeliveredQuantity = d.delivered_quantity,
                        GymNumber = d.Inventory.Gym.gym_number
                    }).OrderBy(d => d.DeliveryDate).ToList()
                }).ToList()
            }).ToList();

            return Ok(result);
        }


        //For filltration
        public class SupplierFilltrationDto
        {
            public int SupplierId { get; set; }
            public string SupplierName { get; set; } = string.Empty;
        }

        public class BrandDto
        {
            public int BrandId { get; set; }
            public string BrandName { get; set; } = string.Empty;
        }

        [HttpGet("all-suppliers")]
        public async Task<ActionResult<IEnumerable<SupplierFilltrationDto>>> GetSuppliers()
        {
            var suppliers = await _context.Suppliers
                .Select(s => new SupplierFilltrationDto
                {
                    SupplierId = s.supplier_id,
                    SupplierName = s.supplier_name
                })
                .OrderBy(s => s.SupplierName)
                .ToListAsync();

            return Ok(suppliers);
        }

        [HttpGet("all-brands")]
        public async Task<ActionResult<IEnumerable<BrandDto>>> GetBrands()
        {
            var brands = await _context.Brands
                .Select(b => new BrandDto
                {
                    BrandId = b.brand_id,
                    BrandName = b.brand_name
                })
                .OrderBy(b => b.BrandName)
                .ToListAsync();

            return Ok(brands);
        }

        public class OrderItemDTO
        {
            public int ProductId { get; set; }
            public int SupplierId { get; set; }
            public int Quantity { get; set; }
            public string ProductModel { get; set; } = string.Empty;
        }

        [HttpPost("create-from-basket")]
        public async Task<IActionResult> CreateFromBasket([FromBody] List<OrderItemDTO> items)
        {
            if (items == null || !items.Any())
                return BadRequest("Порожній список замовлення.");

            var groupedBySupplier = items.GroupBy(i => i.SupplierId);
            var createdOrders = new List<object>();

            foreach (var group in groupedBySupplier)
            {
                var supplierId = group.Key;

                int maxOrderNumber = await _context.Orders.MaxAsync(o => (int?)o.order_number) ?? 0;

                var order = new Order
                {
                    supplier_id = supplierId,
                    order_date = DateTime.Now,
                    order_number = maxOrderNumber + 1,
                    order_status_id = await _context.OrderStatuses
                        .Where(s => s.order_status_name == "В процесі")
                        .Select(s => s.order_status_id)
                        .FirstOrDefaultAsync(),
                    payment_method_id = await _context.PaymentMethods
                        .Select(p => p.payment_method_id)
                        .FirstOrDefaultAsync()
                };

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                var purchasedProducts = new List<PurchasedProduct>();

                foreach (var item in group)
                {
                    var product = await _context.Products.FindAsync(item.ProductId);
                    if (product == null) continue;

                    // Витягуємо останню відому ціну з PurchasedProducts (якщо є), інакше ставимо 0
                    var lastPrice = await _context.PurchasedProducts
                        .Where(pp => pp.product_id == item.ProductId)
                        .OrderByDescending(pp => pp.purchased_product_id)
                        .Select(pp => pp.unit_price)
                        .FirstOrDefaultAsync();

                    purchasedProducts.Add(new PurchasedProduct
                    {
                        order_id = order.order_id,
                        product_id = item.ProductId,
                        quantity = item.Quantity,
                        unit_price = lastPrice > 0 ? lastPrice : 0
                    });
                }

                _context.PurchasedProducts.AddRange(purchasedProducts);

                order.order_total_price = purchasedProducts.Sum(p => p.quantity * p.unit_price);
                await _context.SaveChangesAsync();

                createdOrders.Add(new
                {
                    order.order_id,
                    order.order_number,
                    order.order_date,
                    order.order_total_price
                });
            }

            return Ok(createdOrders);
        }

    }
}
