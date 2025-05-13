using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportComplexAPI.Data;
using SportComplexAPI.DTOs;
using SportComplexAPI.Models;
using SportComplexAPI.Services;

namespace SportComplexAPI.Controllers.InternalManager
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

            var userName = Request.Headers["X-User-Name"].FirstOrDefault() ?? "Anonymous";
            var roleName = Request.Headers["X-User-Role"].FirstOrDefault() ?? "Unknown";
            LogService.LogAction(userName, roleName, $"Додав нового клієнта (ID: {client.client_id})");

            return Ok(new
            {
                ClientId = client.client_id,
                ClientFullName = client.client_full_name,
                ClientPhoneNumber = client.client_phone_number,
                ClientGender = gender.gender_name
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateClient(int id, [FromBody] ClientCreateDto dto)
        {
            var client = await _context.Clients.FindAsync(id);
            if (client == null)
                return NotFound("Клієнт не знайдений.");

            var gender = await _context.Genders
                .FirstOrDefaultAsync(g => g.gender_name == dto.ClientGender);
            if (gender == null)
                return BadRequest($"Гендер '{dto.ClientGender}' не знайдений.");

            client.client_full_name = dto.ClientFullName;
            client.client_phone_number = dto.ClientPhoneNumber;
            client.client_gender_id = gender.gender_id;
            await _context.SaveChangesAsync();

            var userName = Request.Headers["X-User-Name"].FirstOrDefault() ?? "Anonymous";
            var roleName = Request.Headers["X-User-Role"].FirstOrDefault() ?? "Unknown";
            LogService.LogAction(userName, roleName, $"Змінив клієнта (ID: {id})");

            return Ok(new
            {
                ClientId = client.client_id,
                ClientFullName = client.client_full_name,
                ClientPhoneNumber = client.client_phone_number,
                ClientGender = gender.gender_name
            });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteClient(int id)
        {
            var client = await _context.Clients.FindAsync(id);
            if (client == null)
                return NotFound("Клієнт не знайдений.");

            _context.Clients.Remove(client);
            await _context.SaveChangesAsync();

            var userName = Request.Headers["X-User-Name"].FirstOrDefault() ?? "Anonymous";
            var roleName = Request.Headers["X-User-Role"].FirstOrDefault() ?? "Unknown";

            LogService.LogAction(userName, roleName, $"Видалив клієнта (ID: {id})");

            return Ok(new { message = $"Клієнт з ID {id} успішно видалений." });
        }

    }
}
