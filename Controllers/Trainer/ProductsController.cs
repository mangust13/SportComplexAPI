// Controllers/ProductsController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportComplexAPI.Data;
using SportComplexAPI.DTOs;
using static SportComplexAPI.Controllers.PurchaseManager.OrdersController;

namespace SportComplexAPI.Controllers
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
    }
}
