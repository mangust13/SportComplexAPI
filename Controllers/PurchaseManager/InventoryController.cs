using Microsoft.AspNetCore.Mvc;
using SportComplexAPI.Data;
using SportComplexAPI.DTOs.PurchaseManager;
using Microsoft.EntityFrameworkCore;

namespace SportComplexAPI.Controllers.PurchaseManager
{
    [ApiController]
    [Route("api/[controller]")]
    public class InventoryController : ControllerBase
    {
        private readonly SportComplexContext _context;
        public InventoryController(SportComplexContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<InventoryDto>>> GetInventory()
        {
            var list = await _context.Inventory
                .Include(i => i.Gym)
                    .ThenInclude(g => g.SportComplex)
                        .ThenInclude(sc => sc.City)
                .Include(i => i.Product)
                    .ThenInclude(p => p.Brand)
                .Include(i => i.Product)
                    .ThenInclude(p => p.ProductType)
                .Select(i => new InventoryDto
                {
                    ComplexAddress = i.Gym.SportComplex.complex_address,
                    CityName = i.Gym.SportComplex.City.city_name,
                    GymNumber = i.Gym.gym_number,
                    ProductId = i.product_id,
                    BrandName = i.Product.Brand.brand_name,
                    ProductModel = i.Product.product_model,
                    ProductType = i.Product.ProductType.product_type_name,
                    ProductDescription = i.Product.product_description,
                    Quantity = i.quantity
                })
                .ToListAsync();

            return Ok(list);
        }
    }
}
