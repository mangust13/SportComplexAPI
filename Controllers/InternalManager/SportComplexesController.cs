using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportComplexAPI.Data;
using SportComplexAPI.DTOs;
using SportComplexAPI.Models;

namespace SportComplexAPI.Controllers.InternalManager
{
    [ApiController]
    [Route("api/[controller]")]
    public class SportComplexesController : ControllerBase
    {
        private readonly SportComplexContext _context;
        public SportComplexesController(SportComplexContext context)
        {
            _context = context;
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllSportComplexes()
        {
            var complexes = await _context.SportComplexes
                .Include(sc => sc.City)
                .Select(sc => new
                {
                    SportComplexId = sc.sport_complex_id,
                    City = sc.City.city_name,
                    Address = sc.complex_address
                })
                .ToListAsync();

            return Ok(complexes);
        }


        [HttpGet("cities")]
        public async Task<IActionResult> GetCities()
        {
            var cities = await _context.SportComplexes
                .Select(sc => sc.City.city_name)
                .Distinct()
                .ToListAsync();
            return Ok(cities);
        }

        [HttpGet("addresses")]
        public async Task<IActionResult> GetAddresses()
        {
            var addresses = await _context.SportComplexes
                .Select(sc => sc.complex_address)
                .Distinct()
                .ToListAsync();
            return Ok(addresses);
        }

        [HttpGet("id")]
        public async Task<IActionResult> GetSportComplexId([FromQuery] string city, [FromQuery] string address)
        {
            var complex = await _context.SportComplexes
                .FirstOrDefaultAsync(sc => sc.City.city_name == city && sc.complex_address == address);

            if (complex == null)
                return NotFound("Спортивний комплекс не знайдено.");

            return Ok(complex.sport_complex_id);
        }

    }
}
