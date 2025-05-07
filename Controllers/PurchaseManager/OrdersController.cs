using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportComplexAPI.Data;
using SportComplexAPI.DTOs.PurchaseManager;

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

    }
}
