using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportComplexAPI.Data;
using SportComplexAPI.DTOs;
using SportComplexAPI.Models;

namespace SportComplexAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ClientsController : ControllerBase
    {
        private readonly SportComplexContext _context;
        public ClientsController(SportComplexContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllClients()
        {
            var clients = await _context.Clients
                .Select(c => new
                {
                    clientId = c.client_id,
                    clientFullName = c.client_full_name,
                    clientPhoneNumber = c.client_phone_number,
                    clientGender = c.Gender.gender_name
                })
                .ToListAsync();

            return Ok(clients);
        }
        public class ClientCreateDto
        {
            public string ClientFullName { get; set; } = null!;
            public string ClientPhoneNumber { get; set; } = null!;
            public string ClientGender { get; set; } = null!;
        }


        [HttpPost]
        public async Task<IActionResult> CreateClient([FromBody] ClientCreateDto dto)
        {
            var gender = await _context.Genders
                .FirstOrDefaultAsync(g => g.gender_name == dto.ClientGender);
            if (gender == null)
                return BadRequest($"Гендер '{dto.ClientGender}' не знайдений.");

            var client = new Client
            {
                client_full_name = dto.ClientFullName,
                client_phone_number = dto.ClientPhoneNumber,
                client_gender_id = gender.gender_id
            };

            _context.Clients.Add(client);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                Message = "Клієнт успішно створений!",
                ClientId = client.client_id
            });
        }

    }
}
