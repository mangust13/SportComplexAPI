using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportComplexAPI.Data;
using SportComplexAPI.DTOs;
using SportComplexAPI.Models;
using System.Security.Cryptography;
using System.Text;

namespace SportComplexAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly SportComplexContext _context;
        public AuthController(SportComplexContext context)
        {
            _context = context;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            try
            {
                var user = _context.Users
                    .Include(u => u.Role)
                    .SingleOrDefault(u => u.UserName == request.UserName);


                if (user == null) return Unauthorized("Invalid username");

                var sha256 = SHA256.Create();
                var hashed = Convert.ToBase64String(sha256.ComputeHash(Encoding.UTF8.GetBytes(request.Password)));

                if (hashed != user.PasswordHash)
                    return Unauthorized("Invalid password");

                return Ok(new
                {
                    username = user.UserName,
                    role = user.Role.RoleName
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"💥 Server error: {ex.Message}");
            }
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (await _context.Users.AnyAsync(u => u.UserName == request.UserName))
                return BadRequest("Username already exists");

            var role = await _context.UserRoles.SingleOrDefaultAsync(r => r.RoleName == request.RoleName);
            if (role == null)
                return BadRequest("Invalid role");

            var sha256 = SHA256.Create();
            var hashed = Convert.ToBase64String(sha256.ComputeHash(Encoding.UTF8.GetBytes(request.Password)));

            var user = new User
            {
                UserName = request.UserName,
                PasswordHash = hashed,
                RoleId = role.RoleId
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "User registered successfully" });
        }
    }
}