using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportComplexAPI.Data;
using SportComplexAPI.Models;
using SportComplexAPI.Services;
using static SportComplexAPI.Controllers.PurchaseManager.OrdersController;

namespace SportComplexAPI.Controllers.PurchaseManager
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly SportComplexContext _context;
        public ProductsController(SportComplexContext context) => _context = context;

        [HttpGet("all-brands")]
        public async Task<IActionResult> GetBrands() =>
            Ok(await _context.Brands
                .Select(b => new BrandDto
                {
                    BrandId = b.brand_id,
                    BrandName = b.brand_name
                })
                .ToListAsync());

        [HttpGet("all-types")]
        public async Task<IActionResult> GetTypes() =>
            Ok(await _context.ProductTypes
                .Select(t => new
                {
                    ProductTypeId = t.product_type_id,
                    ProductTypeName = t.product_type_name
                })
                .ToListAsync());

        [HttpGet("products-view")]
        public async Task<IActionResult> GetProductsView(
            [FromQuery] int? brandId,
            [FromQuery] int? typeId)
        {
            var q = _context.Products
                .Include(p => p.Brand)
                .Include(p => p.ProductType)
                .AsQueryable();

            if (brandId.HasValue)
                q = q.Where(p => p.brand_id == brandId.Value);
            if (typeId.HasValue)
                q = q.Where(p => p.product_type_id == typeId.Value);

            var list = await q
                .Select(p => new
                {
                    ProductId = p.product_id,
                    BrandId = p.brand_id,
                    BrandName = p.Brand.brand_name,
                    ProductTypeId = p.product_type_id,
                    ProductTypeName = p.ProductType.product_type_name,
                    ProductModel = p.product_model,
                    ProductDescription = p.product_description
                })
                .ToListAsync();

            return Ok(list);
        }

        public class CreateUpdateProductDto
        {
            public string ProductModel { get; set; } = null!;
            public string ProductDescription { get; set; } = null!;
            public int BrandId { get; set; }
            public int ProductTypeId { get; set; }
        }

        [HttpPost]
        public async Task<IActionResult> AddProduct([FromBody] CreateUpdateProductDto dto)
        {
            var product = new Product
            {
                product_model = dto.ProductModel,
                product_description = dto.ProductDescription,
                brand_id = dto.BrandId,
                product_type_id = dto.ProductTypeId
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            var result = await _context.Products
                .Include(p => p.Brand)
                .Include(p => p.ProductType)
                .Where(p => p.product_id == product.product_id)
                .Select(p => new 
                {
                    ProductId = p.product_id,
                    ProductModel = p.product_model,
                    ProductDescription = p.product_description,
                    BrandId = p.brand_id,
                    BrandName = p.Brand.brand_name,
                    ProductTypeId = p.product_type_id,
                    ProductTypeName = p.ProductType.product_type_name
                })
                .FirstAsync();

            return Ok(result);
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] CreateUpdateProductDto dto)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
                return NotFound("Продукт не знайдено");

            product.product_model = dto.ProductModel;
            product.product_description = dto.ProductDescription;
            product.brand_id = dto.BrandId;
            product.product_type_id = dto.ProductTypeId;

            await _context.SaveChangesAsync();

            var userName = Request.Headers["X-User-Name"].FirstOrDefault() ?? "Anonymous";
            var roleName = Request.Headers["X-User-Role"].FirstOrDefault() ?? "Unknown";
            LogService.LogAction(userName, roleName, $"Оновив інформацю про спортивний товар (ID: {id})");
            return Ok("Продукт оновлено");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
                return NotFound("Продукт не знайдено");

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            var userName = Request.Headers["X-User-Name"].FirstOrDefault() ?? "Anonymous";
            var roleName = Request.Headers["X-User-Role"].FirstOrDefault() ?? "Unknown";
            LogService.LogAction(userName, roleName, $"Видалив інформацю про спортивний товар (ID: {id})");
            return Ok("Продукт успішно видалено");
        }

    }
}
