using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportComplexAPI.Data;
using SportComplexAPI.DTOs.InternalManager;
using SportComplexAPI.Models;
using SportComplexAPI.Services;

namespace SportComplexAPI.Controllers.InternalManager
{
    [ApiController]
    [Route("api/[controller]")]
    public class ActivitiesController : ControllerBase
    {
        private readonly SportComplexContext _context;
        public ActivitiesController(SportComplexContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllActivities()
        {
            var activities = await _context.Activities
                .Select(a => new
                {
                    ActivityId = a.activity_id,
                    ActivityName = a.activity_name,
                    ActivityPrice = a.activity_price,
                    ActivityDescription = a.activity_description
                })
                .ToListAsync();

            return Ok(activities);
        }

        [HttpPost]
        public async Task<IActionResult> CreateActivity([FromBody] ActivityCreateUpdateDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.ActivityName) || dto.ActivityPrice <= 0)
            {
                return BadRequest("Некоректні дані активності.");
            }

            var newActivity = new Activity
            {
                activity_name = dto.ActivityName,
                activity_price = dto.ActivityPrice,
                activity_description = dto.ActivityDescription
            };

            _context.Activities.Add(newActivity);
            await _context.SaveChangesAsync();

            var userName = Request.Headers["X-User-Name"].FirstOrDefault() ?? "Anonymous";
            var roleName = Request.Headers["X-User-Role"].FirstOrDefault() ?? "Unknown";
            LogService.LogAction(userName, roleName, $"Додано нову активність (ID: {newActivity.activity_id}, Назва: {newActivity.activity_name})");

            return Ok(new
            {
                ActivityId = newActivity.activity_id,
                ActivityName = newActivity.activity_name,
                ActivityPrice = newActivity.activity_price,
                ActivityDescription = newActivity.activity_description
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateActivity(int id, [FromBody] ActivityCreateUpdateDTO dto)
        {
            var activity = await _context.Activities.FindAsync(id);
            if (activity == null)
                return NotFound("Активність не знайдена.");

            activity.activity_name = dto.ActivityName;
            activity.activity_price = dto.ActivityPrice;
            activity.activity_description = dto.ActivityDescription;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                ActivityId = activity.activity_id,
                ActivityName = activity.activity_name,
                ActivityPrice = activity.activity_price,
                ActivityDescription = activity.activity_description
            });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteActivity(int id)
        {
            var activity = await _context.Activities.FindAsync(id);
            if (activity == null)
                return NotFound("Активність не знайдена.");

            _context.Activities.Remove(activity);
            await _context.SaveChangesAsync();

            var userName = Request.Headers["X-User-Name"].FirstOrDefault() ?? "Anonymous";
            var roleName = Request.Headers["X-User-Role"].FirstOrDefault() ?? "Unknown";
            LogService.LogAction(userName, roleName, $"Видалив активність (ID: {id}, Назва: {activity.activity_name})");

            return Ok(new { message = "Активність успішно видалена." });
        }

    }
}
