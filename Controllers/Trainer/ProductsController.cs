using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportComplexAPI.Data;
using SportComplexAPI.Models;
using System.Linq;

namespace SportComplexAPI.Controllers.Trainer
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly SportComplexContext _context;
        public ProductsController(SportComplexContext context)
        {
            _context = context;
        }

        [HttpGet("products-view")]
        public async Task<IActionResult> GetProductsView()
        {
            var types = await _context.Products
                .Select( p => p.ProductType.product_type_name)
                .Distinct().ToListAsync();

            var products = await _context.Products
                .Include(p => p.Brand)
                .Include(p => p.ProductType)
                .Select(p => new
                {
                    ProductId = p.product_id,
                    ProductModel = p.product_model,
                    BrandName = p.Brand.brand_name,
                    ProductType = p.ProductType.product_type_name,
                    ProductDescription = p.product_description
                })
                .ToListAsync();

            Console.Write(types);
            return Ok(products);
        }
    }
}