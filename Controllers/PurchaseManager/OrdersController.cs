using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportComplexAPI.Data;
using SportComplexAPI.DTOs.PurchaseManager;
using SportComplexAPI.Models;
using SportComplexAPI.Services;
using System.Linq;

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
            [FromQuery] string? sortBy, [FromQuery] string? order,
            [FromQuery] int? supplierId, [FromQuery] int? brandId,
            [FromQuery] decimal? minTotal,[FromQuery] decimal? maxTotal,
            [FromQuery] DateTime? orderDate)
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
                                .ThenInclude(g => g.SportComplex)
                                    .ThenInclude(sc => sc.City)
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
                    _ => query.OrderByDescending(o => o.order_date)
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
                    ProductId = pp.product_id,
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
                        GymNumber = d.Inventory.Gym.gym_number,
                        ComplexAddress = d.Inventory.Gym.SportComplex.complex_address,
                        ComplexCity = d.Inventory.Gym.SportComplex.City.city_name
                    }).OrderBy(d => d.DeliveryDate).ToList()
                }).ToList()
            }).ToList();

            return Ok(result);
        }


        [HttpGet("all-suppliers")]
        public async Task<ActionResult<IEnumerable<SupplierDto>>> GetSuppliers()
        {
            var suppliers = await _context.Suppliers
                .Select(s => new SupplierDto
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
            public decimal UnitPrice { get; set; } // 🟢 додано
        }

        public class OrderFromBasketDto
        {
            public int SupplierId { get; set; }
            public List<OrderItemDTO> Items { get; set; } = new();
        }

        [HttpPost("create-from-basket")]
        public async Task<IActionResult> CreateFromBasket([FromBody] OrderFromBasketDto dto)
        {
            int maxOrderNumber = await _context.Orders.MaxAsync(o => (int?)o.order_number) ?? 0;

            var order = new Order
            {
                supplier_id = dto.SupplierId,
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

            var purchasedProducts = dto.Items.Select(item => new PurchasedProduct
            {
                order_id = order.order_id,
                product_id = item.ProductId,
                quantity = item.Quantity,
                unit_price = item.UnitPrice
            }).ToList();

            _context.PurchasedProducts.AddRange(purchasedProducts);

            order.order_total_price = purchasedProducts.Sum(p => p.quantity * p.unit_price);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                order.order_id,
                order.order_number,
                order.order_date,
                order.order_total_price
            });
        }

        public class AddDeliveryDto
        {
            public int PurchasedProductId { get; set; }
            public int GymId { get; set; }
            public DateTime DeliveryDate { get; set; }
            public int DeliveredQuantity { get; set; }
        }

        [HttpPost("add-delivery")]
        public async Task<IActionResult> AddDelivery([FromBody] AddDeliveryDto dto)
        {
            var purchasedProduct = await _context.PurchasedProducts
                .Include(pp => pp.Deliveries)
                .Include(pp => pp.Order)
                .FirstOrDefaultAsync(pp => pp.purchased_product_id == dto.PurchasedProductId);

            if (purchasedProduct == null)
                return NotFound("Товар у замовленні не знайдено");

            var gym = await _context.Gyms
                .Include(g => g.SportComplex)
                .ThenInclude(sc => sc.City)
                .FirstOrDefaultAsync(g => g.gym_id == dto.GymId);

            if (gym == null)
                return BadRequest("Зал не знайдено");

            if (dto.DeliveredQuantity <= 0 || dto.DeliveredQuantity > (purchasedProduct.quantity - purchasedProduct.Deliveries.Sum(d => d.delivered_quantity)))
                return BadRequest("Некоректна кількість поставки");

            var inventory = new Inventory
            {
                gym_id = dto.GymId,
                product_id = purchasedProduct.product_id,
                quantity = dto.DeliveredQuantity
            };

            _context.Inventory.Add(inventory);
            await _context.SaveChangesAsync();

            var delivery = new Delivery
            {
                purchased_product_id = purchasedProduct.purchased_product_id,
                delivery_date = dto.DeliveryDate,
                delivered_quantity = dto.DeliveredQuantity,
                inventory_id = inventory.inventory_id
            };

            _context.Deliveries.Add(delivery);
            await _context.SaveChangesAsync();

            var order = purchasedProduct.Order;
            var allDelivered = await _context.PurchasedProducts
                .Where(pp => pp.order_id == order.order_id)
                .AllAsync(pp =>
                    pp.Deliveries.Sum(d => d.delivered_quantity) >= pp.quantity
                );

            if (allDelivered)
            {
                var completedStatus = await _context.OrderStatuses
                    .FirstOrDefaultAsync(s => s.order_status_name == "Виконане");

                if (completedStatus != null)
                {
                    order.order_status_id = completedStatus.order_status_id;
                    await _context.SaveChangesAsync();
                }
            }

            return Ok(new
            {
                deliveryId = delivery.delivery_id,
                deliveryDate = delivery.delivery_date,
                deliveredQuantity = delivery.delivered_quantity,
                gymNumber = gym.gym_number,
                complexAddress = gym.SportComplex.complex_address,
                complexCity = gym.SportComplex.City.city_name,
                orderStatus = allDelivered ? "Виконане" : "В процесі"
            }); ;
        }

        [HttpGet("gyms")]
        public async Task<IActionResult> GetGyms()
        {
            var gyms = await _context.Gyms
                .Include(g => g.SportComplex)
                    .ThenInclude(sc => sc.City)
                .Select(g => new
                {
                    GymId = g.gym_id,
                    GymNumber = g.gym_number,
                    ComplexAddress = g.SportComplex.complex_address,
                    ComplexCity = g.SportComplex.City.city_name
                })
                .ToListAsync();

            return Ok(gyms);
        }

        public class UpdatePurchasedProductDto
        {
            public int PurchasedProductId { get; set; }
            public int ProductId { get; set; }
            public int Quantity { get; set; } 
        }


        [HttpPut("update-products/{orderId}")]
        public async Task<IActionResult> UpdateOrderProducts(int orderId, [FromBody] List<UpdatePurchasedProductDto> dtos)
        {
            var order = await _context.Orders
                .Include(o => o.PurchasedProducts)
                    .ThenInclude(pp => pp.Deliveries)
                .FirstOrDefaultAsync(o => o.order_id == orderId);

            if (order == null)
                return NotFound("Замовлення не знайдено");

            var updatedIds = dtos.Where(d => d.PurchasedProductId != 0).Select(d => d.PurchasedProductId).ToList();
            var toRemove = order.PurchasedProducts
                .Where(pp => !updatedIds.Contains(pp.purchased_product_id) && !pp.Deliveries.Any())
                .ToList();

            _context.PurchasedProducts.RemoveRange(toRemove);

            foreach (var dto in dtos)
            {
                if (dto.PurchasedProductId == 0)
                {
                    _context.PurchasedProducts.Add(new PurchasedProduct
                    {
                        order_id = orderId,
                        product_id = dto.ProductId,
                        quantity = dto.Quantity,
                        unit_price = 0
                    });
                }
                else
                {
                    var existing = order.PurchasedProducts.FirstOrDefault(p => p.purchased_product_id == dto.PurchasedProductId);
                    if (existing != null)
                    {
                        existing.product_id = dto.ProductId;
                        existing.quantity = dto.Quantity;
                    }
                }
            }

            await _context.SaveChangesAsync();

            var userName = Request.Headers["X-User-Name"].FirstOrDefault() ?? "Anonymous";
            var roleName = Request.Headers["X-User-Role"].FirstOrDefault() ?? "Unknown";
            LogService.LogAction(userName, roleName, $"Оновив товари в замовленні (ID: {orderId})");

            return Ok("Товари замовлення оновлено");
        }


    }
}
