using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportComplexAPI.Data;
using SportComplexAPI.Models;

namespace SportComplexAPI.Controllers.Trainer
{
    [ApiController]
    [Route("api/[controller]")]
    public class ConcreteTrainersController : ControllerBase
    {
        private readonly SportComplexContext _context;
        public ConcreteTrainersController(SportComplexContext context)
        {
            _context = context;
        }
        [HttpGet("{trainerId}/profile")]
        public async Task<IActionResult> GetTrainerProfile(int trainerId)
        {
            var trainer = await _context.Trainers
                .Include(t => t.TrainerActivities)
                    .ThenInclude(ta => ta.Activity)
                .SingleOrDefaultAsync(t => t.trainer_id == trainerId);

            if (trainer == null)
                return NotFound("Trainer not found");

            // Завантажити розклади ТІЛЬКИ для цього тренера окремо
            var trainerSchedules = await _context.TrainerSchedules
                .Where(ts => ts.trainer_id == trainerId)
                .Include(ts => ts.Schedule)
                    .ThenInclude(s => s.DayOfWeek)
                .Include(ts => ts.ActivityInGym)
                    .ThenInclude(aig => aig.Activity)
                .Include(ts => ts.ActivityInGym)
                    .ThenInclude(aig => aig.Gym)
                        .ThenInclude(g => g.SportComplex)
                            .ThenInclude(sc => sc.City)
                .ToListAsync();

            var profileDto = new TrainerProfileDto
            {
                TrainerFullName = trainer.trainer_full_name,
                GymNumber = trainerSchedules.FirstOrDefault()?.ActivityInGym.Gym.gym_number ?? 0,

                Activities = trainer.TrainerActivities
                    .Select(ta => new ActivityDto
                    {
                        ActivityId = ta.activity_id,
                        ActivityName = ta.Activity.activity_name,
                    })
                    .ToList(),

                Schedules = trainerSchedules
                    .Select(ts => new TrainerScheduleEntryDto
                    {
                        TrainerScheduleId = ts.trainer_schedule_id,
                        ScheduleId = ts.schedule_id,
                        DayName = ts.Schedule.DayOfWeek.day_name,
                        StartTime = ts.Schedule.start_time.ToString(@"hh\:mm"),
                        EndTime = ts.Schedule.end_time.ToString(@"hh\:mm"),
                        ActivityId = ts.ActivityInGym.Activity.activity_id,
                        ActivityName = ts.ActivityInGym.Activity.activity_name,
                        GymNumber = ts.ActivityInGym.Gym.gym_number,
                        SportComplexAddress = ts.ActivityInGym.Gym.SportComplex.complex_address,
                        SportComplexCity = ts.ActivityInGym.Gym.SportComplex.City.city_name,
                        TrainerId = ts.trainer_id
                    })
                    .ToList()
            };

            return Ok(profileDto);
        }


        public class TrainerProfileDto
        {
            public string TrainerFullName { get; set; } = null!;
            public int GymNumber { get; set; }

            public List<ActivityDto> Activities { get; set; } = new();
            public List<TrainerScheduleEntryDto> Schedules { get; set; } = new();
        }

        public class ActivityDto
        {
            public int ActivityId { get; set; }
            public string ActivityName { get; set; } = null!;
        }

        public class TrainerScheduleEntryDto
        {
            public int TrainerScheduleId { get; set; }
            public int ScheduleId { get; set; }
            public string DayName { get; set; } = null!;
            public string StartTime { get; set; } = null!;
            public string EndTime { get; set; } = null!;
            public int ActivityId { get; set; }
            public string ActivityName { get; set; } = null!;
            public int GymNumber { get; set; }
            public string SportComplexAddress { get; set; } = null!;
            public string SportComplexCity { get; set; } = null!;
            public int TrainerId { get; set; }
        }

        [HttpGet("eligible-clients")]
        public async Task<IActionResult> GetEligibleClientsForSchedule([FromQuery] int activityId)
        {
            var clients = await _context.Clients
                .Include(c => c.Gender)
                .Include(c => c.Purchases)
                    .ThenInclude(p => p.Subscription)
                        .ThenInclude(s => s.SubscriptionActivities)
                .Include(c => c.Purchases)
                    .ThenInclude(p => p.Subscription)
                        .ThenInclude(s => s.BaseSubscription)
                            .ThenInclude(bs => bs.SubscriptionTerm)
                .Include(c => c.Purchases)
                    .ThenInclude(p => p.AttendanceRecords)
                .ToListAsync(); // ← до цього моменту EF може все згенерувати

            var result = clients
                .Where(c => c.Purchases.Any(p =>
                    !IsExpired(p) &&
                    HasTrainingsLeft(p) &&
                    p.Subscription.SubscriptionActivities.Any(sa => sa.activity_id == activityId)
                ))
                .Select(c => new ClientWithPurchasesDto
                {
                    ClientId = c.client_id,
                    ClientFullName = c.client_full_name,
                    ClientPhoneNumber = c.client_phone_number,
                    ClientGender = c.Gender.gender_name,
                    Purchases = c.Purchases
                        .Where(p =>
                            !IsExpired(p) &&
                            HasTrainingsLeft(p) &&
                            p.Subscription.SubscriptionActivities.Any(sa => sa.activity_id == activityId)
                        )
                        .Select(p => new PurchaseShortDto
                        {
                            PurchaseId = p.purchase_id,
                            PurchaseNumber = p.purchase_number,
                            SubscriptionName = p.Subscription.subscription_name
                        })
                        .ToList()
                })
                .ToList();

            return Ok(result);
        }


        public class ClientWithPurchasesDto
        {
            public int ClientId { get; set; }
            public string ClientFullName { get; set; } = null!;
            public string ClientPhoneNumber { get; set; } = null!;
            public string ClientGender { get; set; } = null!;
            public List<PurchaseShortDto> Purchases { get; set; } = new();
        }

        public class PurchaseShortDto
        {
            public int PurchaseId { get; set; }
            public int PurchaseNumber { get; set; }
            public string SubscriptionName { get; set; } = null!;
        }

        private bool IsExpired(Purchase p)
        {
            var termText = p.Subscription.BaseSubscription.SubscriptionTerm.subscription_term.ToLower();
            int months = 0;

            if (termText.Contains("рік")) months = 12;
            else if (termText.Contains("місяц")) months = int.TryParse(new string(termText.Where(char.IsDigit).ToArray()), out var m) ? m : 0;

            var expirationDate = p.purchase_date.AddMonths(months);
            return DateTime.Now > expirationDate;
        }

        private bool HasTrainingsLeft(Purchase p)
        {
            var total = p.Subscription.SubscriptionActivities.Sum(sa => sa.activity_type_amount);
            var used = p.AttendanceRecords.Count;
            return total > used;
        }

        public class AddAttendanceDto
        {
            public int ClientId { get; set; }
            public int PurchaseId { get; set; }
            public int TrainerScheduleId { get; set; }
        }

        [HttpPost("add-attendance")]
        public async Task<IActionResult> AddAttendance([FromBody] AddAttendanceDto dto)
        {
            var schedule = await _context.TrainerSchedules
                .Include(ts => ts.Schedule)
                .FirstOrDefaultAsync(ts => ts.trainer_schedule_id == dto.TrainerScheduleId);

            if (schedule == null)
                return BadRequest("Розклад не знайдено");

            var training = await _context.Trainings
                .FirstOrDefaultAsync(t => t.trainer_schedule_id == dto.TrainerScheduleId);

            if (training == null)
            {
                training = new Training
                {
                    trainer_schedule_id = dto.TrainerScheduleId,
                    training_start_time = schedule.Schedule.start_time,
                    training_end_time = schedule.Schedule.end_time
                };
                _context.Trainings.Add(training);
                await _context.SaveChangesAsync();
            }

            var attendance = new AttendanceRecord
            {
                purchase_id = dto.PurchaseId,
                training_id = training.training_id,
                attendance_date_time = DateTime.Now
            };

            _context.AttendanceRecords.Add(attendance);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Відвідування успішно додано." });
        }

    }
}
