using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportComplexAPI.Data;
using SportComplexAPI.DTOs.Trainer;
using SportComplexAPI.Models;
using SportComplexAPI.Services;

namespace SportComplexAPI.Controllers.Trainer
{
    [ApiController]
    [Route("api/[controller]")]
    public class AttendanceRecordController : ControllerBase
    {
        private readonly SportComplexContext _context;
        public AttendanceRecordController(SportComplexContext context)
        {
            _context = context;
        }

        [HttpGet("attendances")]
        public async Task<IActionResult> GetAttendanceForTrainer(
            [FromQuery] int trainerId, [FromQuery] DateTime? date = null,
            [FromQuery] string? activities = null, [FromQuery] string? sortBy = null,
            [FromQuery] string? order = null, [FromQuery] string? purchaseNumber = null)
        {
            var query = _context.AttendanceRecords
                .Include(a => a.Purchase)
                    .ThenInclude(p => p.Client)
                .Include(a => a.Purchase)
                    .ThenInclude(p => p.Subscription)
                        .ThenInclude(s => s.SubscriptionActivities)
                            .ThenInclude(sa => sa.Activity)
                .Include(a => a.Training)
                    .ThenInclude(t => t.TrainerSchedule)
                        .ThenInclude(ts => ts.ActivityInGym)
                            .ThenInclude(aig => aig.Activity)
                .Where(a => a.Training.TrainerSchedule.trainer_id == trainerId)
                .AsQueryable();

            if (date.HasValue)
                query = query.Where(a => a.attendance_date_time.Date == date.Value.Date);

            if (!string.IsNullOrEmpty(activities))
            {
                var activityList = activities.Split(',');
                query = query.Where(a => a.Purchase.Subscription.SubscriptionActivities
                    .Any(sa => activityList.Contains(sa.Activity.activity_name)));
            }

            if (int.TryParse(purchaseNumber, out var purchaseNum))
                query = query.Where(a => a.Purchase.purchase_number == purchaseNum);

            // Сортування
            if (!string.IsNullOrEmpty(sortBy))
            {
                if (sortBy == "purchaseNumber")
                {
                    query = order == "asc"
                        ? query.OrderBy(a => a.Purchase.purchase_number)
                        : query.OrderByDescending(a => a.Purchase.purchase_number);
                }
                else if (sortBy == "attendanceDateTime")
                {
                    query = order == "asc"
                        ? query.OrderBy(a => a.attendance_date_time)
                        : query.OrderByDescending(a => a.attendance_date_time);
                }
            }

            var attendances = await query.Select(a => new AttendanceRecordDto
            {
                AttendanceId = a.attendance_id,
                PurchaseNumber = a.Purchase.purchase_number,
                PurchaseDate = a.Purchase.purchase_date,
                ClientFullName = a.Purchase.Client.client_full_name,
                SubscriptionName = a.Purchase.Subscription.subscription_name,
                SubscriptionTerm = a.Purchase.Subscription.BaseSubscription.SubscriptionTerm.subscription_term,
                SubscriptionVisitTime = a.Purchase.Subscription.BaseSubscription.SubscriptionVisitTime.subscription_visit_time,
                SubscriptionActivities = a.Purchase.Subscription.SubscriptionActivities
                    .Select(sa => new ActivityDto
                    {
                        ActivityId = sa.activity_id,
                        ActivityName = sa.Activity.activity_name
                    }).ToList(),
                GymNumber = a.Training.TrainerSchedule.ActivityInGym.Gym.gym_number,
                SportComplexAddress = a.Training.TrainerSchedule.ActivityInGym.Gym.SportComplex.complex_address,
                SportComplexCity = a.Training.TrainerSchedule.ActivityInGym.Gym.SportComplex.City.city_name,
                AttendanceDateTime = a.attendance_date_time,
                TrainingStartTime = a.Training.training_start_time,
                TrainingEndTime = a.Training.training_end_time,
                TrainingActivity = a.Training.TrainerSchedule.ActivityInGym.Activity.activity_name
            }).ToListAsync();

            return Ok(attendances);
        }

        public class UpdateAttendanceDto
        {
            public int AttendanceId { get; set; }
            public DateTime AttendanceDateTime { get; set; }
            public int ActivityInGymId { get; set; }
        }


        [HttpPut("update")]
        public async Task<IActionResult> UpdateAttendance([FromBody] UpdateAttendanceDto dto)
        {
            var attendance = await _context.AttendanceRecords
                .Include(a => a.Training)
                    .ThenInclude(t => t.TrainerSchedule)
                        .ThenInclude(ts => ts.Schedule)
                .FirstOrDefaultAsync(a => a.attendance_id == dto.AttendanceId);

            if (attendance == null)
                return NotFound("Відвідування не знайдено");

            var existingSchedule = attendance.Training.TrainerSchedule;
            existingSchedule.activity_in_gym_id = dto.ActivityInGymId;
            attendance.attendance_date_time = dto.AttendanceDateTime;

            await _context.SaveChangesAsync();

            var userName = Request.Headers["X-User-Name"].FirstOrDefault() ?? "Anonymous";
            var roleName = Request.Headers["X-User-Role"].FirstOrDefault() ?? "Unknown";
            LogService.LogAction(userName, roleName, $"Змінив відвідування (ID: {dto.AttendanceId})");

            return Ok("Відвідування оновлено");
        }


        [HttpDelete("{attendanceId}")]
        public async Task<IActionResult> DeleteAttendance(int attendanceId)
        {
            var attendance = await _context.AttendanceRecords
                .Include(a => a.Training)
                .FirstOrDefaultAsync(a => a.attendance_id == attendanceId);

            if (attendance == null)
                return NotFound("Відвідування не знайдено");

            var training = attendance.Training;

            _context.AttendanceRecords.Remove(attendance);

            await _context.SaveChangesAsync();

            var userName = Request.Headers["X-User-Name"].FirstOrDefault() ?? "Anonymous";
            var roleName = Request.Headers["X-User-Role"].FirstOrDefault() ?? "Unknown";
            LogService.LogAction(userName, roleName, $"Видалив відвідування (ID: {attendanceId})");

            return Ok("Відвідування видалено");
        }


    }
}
