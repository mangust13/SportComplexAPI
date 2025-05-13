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
    public class TrainersController : ControllerBase
    {
        private readonly SportComplexContext _context;
        public TrainersController(SportComplexContext context)
        {
            _context = context;
        }

        [HttpGet("full-schedules")]
        public async Task<IActionResult> GetAllTrainersWithFullSchedules(
            string? gender = null,
            string? cities = null,
            string? addresses = null)
        {
            var query = _context.Trainers
                .Include(t => t.Gender)
                .Include(t => t.SportComplex)
                    .ThenInclude(sc => sc.City)
                .AsQueryable();

            if (!string.IsNullOrEmpty(gender))
                query = query.Where(t => t.Gender.gender_name == gender);

            if (cities != null && cities.Any())
                query = query.Where(t => cities.Contains(t.SportComplex.City.city_name));

            if (addresses != null && addresses.Any())
                query = query.Where(t => addresses.Contains(t.SportComplex.complex_address));

            var trainers = await query.ToListAsync();
            var trainerIds = trainers.Select(t => t.trainer_id).ToList();

            var schedules = await _context.TrainerSchedules
                .Where(ts => trainerIds.Contains(ts.trainer_id))
                .Include(ts => ts.ActivityInGym)
                    .ThenInclude(aig => aig.Activity)
                .Include(ts => ts.Schedule)
                    .ThenInclude(s => s.DayOfWeek)
                .ToListAsync();

            var result = trainers.Select(trainer => new TrainerFullScheduleDto
            {
                TrainerId = trainer.trainer_id,
                TrainerFullName = trainer.trainer_full_name,
                TrainerPhoneNumber = trainer.trainer_phone_number,
                TrainerGender = trainer.Gender.gender_name,
                TrainerAddress = trainer.SportComplex.complex_address,
                TrainerCity = trainer.SportComplex.City.city_name,
                Schedule = schedules
                    .Where(s => s.trainer_id == trainer.trainer_id)
                    .Select(s => new TrainerScheduleEntryDto
                    {
                        ScheduleId = s.schedule_id,
                        DayName = s.Schedule.DayOfWeek.day_name,
                        ActivityName = s.ActivityInGym.Activity.activity_name,
                        StartTime = s.Schedule.start_time.ToString(@"hh\:mm"),
                        EndTime = s.Schedule.end_time.ToString(@"hh\:mm")
                    })
                    .ToList()
            }).ToList();

            return Ok(result);
        }

        [HttpGet("trainer/{trainerId}")]
        public async Task<IActionResult> GetActivitiesForTrainer(int trainerId)
        {
            var trainer = await _context.Trainers
                .Include(t => t.SportComplex)
                .FirstOrDefaultAsync(t => t.trainer_id == trainerId);

            if (trainer == null)
                return NotFound($"Тренера з ID {trainerId} не знайдено.");

            var gymIds = await _context.Gyms
                .Where(g => g.sport_complex_id == trainer.sport_complex_id)
                .Select(g => g.gym_id)
                .ToListAsync();

            if (!gymIds.Any())
                return BadRequest("У спорткомплексі тренера не знайдено жодного залу.");

            var activityIds = await _context.TrainerActivities
                .Where(ta => ta.trainer_id == trainerId)
                .Select(ta => ta.activity_id)
                .Distinct()
                .ToListAsync();

            var activitiesInGyms = await _context.ActivityInGyms
                .Where(aig => gymIds.Contains(aig.gym_id) && activityIds.Contains(aig.activity_id))
                .Select(aig => aig.Activity)
                .Distinct()
                .ToListAsync();

            var result = activitiesInGyms.Select(a => new
            {
                ActivityId = a.activity_id,
                ActivityName = a.activity_name
            }).ToList();

            return Ok(result);
        }

        public class AddTrainerDto
        {
            public string TrainerFullName { get; set; } = null!;
            public string TrainerPhoneNumber { get; set; } = null!;
            public string Gender { get; set; } = null!;
            public int SportComplexId { get; set; }
            public List<int> ActivityIds { get; set; } = new();
        }

        [HttpPost]
        public async Task<IActionResult> AddTrainer([FromBody] AddTrainerDto dto)
        {
            var gender = await _context.Genders.FirstOrDefaultAsync(g => g.gender_name == dto.Gender);
            if (gender == null)
                return BadRequest($"Стать '{dto.Gender}' не знайдена.");

            var sportComplex = await _context.SportComplexes.FirstOrDefaultAsync(sc => sc.sport_complex_id == dto.SportComplexId);
            if (sportComplex == null)
                return BadRequest($"Спортивний комплекс з ID {dto.SportComplexId} не знайдено.");

            var activities = await _context.Activities
                .Where(a => dto.ActivityIds.Contains(a.activity_id))
                .ToListAsync();

            if (activities.Count != dto.ActivityIds.Count)
                return BadRequest("Деякі активності не знайдено.");

            var newTrainer = new SportComplexAPI.Models.Trainer
            {
                trainer_full_name = dto.TrainerFullName,
                trainer_phone_number = dto.TrainerPhoneNumber,
                trainer_gender_id = gender.gender_id,
                sport_complex_id = sportComplex.sport_complex_id,
            };

            _context.Trainers.Add(newTrainer);
            await _context.SaveChangesAsync();

            foreach (var activity in activities)
            {
                _context.TrainerActivities.Add(new TrainerActivity
                {
                    trainer_id = newTrainer.trainer_id,
                    activity_id = activity.activity_id
                });
            }

            await _context.SaveChangesAsync();

            return Ok(new { Message = "Тренера успішно додано!", TrainerId = newTrainer.trainer_id });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTrainer(int id)
        {
            var trainer = await _context.Trainers.FindAsync(id);
            if (trainer == null)
                return NotFound($"Тренера з ID {id} не знайдено.");

            var trainerSchedules = await _context.TrainerSchedules
                .Where(ts => ts.trainer_id == id)
                .ToListAsync();

            var scheduleIds = trainerSchedules.Select(ts => ts.schedule_id).ToList();

            var trainings = await _context.Trainings
                .Where(t => scheduleIds.Contains(t.trainer_schedule_id))
                .ToListAsync();
            _context.Trainings.RemoveRange(trainings);

            var trainerActivities = await _context.TrainerActivities
                .Where(ta => ta.trainer_id == id)
                .ToListAsync();
            _context.TrainerActivities.RemoveRange(trainerActivities);

            _context.TrainerSchedules.RemoveRange(trainerSchedules);
            _context.Trainers.Remove(trainer);
            await _context.SaveChangesAsync();

            var userName = Request.Headers["X-User-Name"].FirstOrDefault() ?? "Anonymous";
            var roleName = Request.Headers["X-User-Role"].FirstOrDefault() ?? "Unknown";
            LogService.LogAction(userName, roleName, $"Видалив дані тренера (ID: {id})");
            return Ok(new { Message = "Тренера успішно видалено!" });
        }

        public class UpdateTrainerDto
        {
            public string TrainerFullName { get; set; } = null!;
            public string TrainerPhoneNumber { get; set; } = null!;
            public string Gender { get; set; } = null!;
            public int SportComplexId { get; set; }
        }

        [HttpPut("{trainerId}")]
        public async Task<IActionResult> UpdateTrainer(int trainerId, [FromBody] UpdateTrainerDto dto)
        {
            var trainer = await _context.Trainers.FindAsync(trainerId);
            if (trainer == null) return NotFound();

            var gender = await _context.Genders.FirstOrDefaultAsync(g => g.gender_name == dto.Gender);
            if (gender == null) return BadRequest("Невідома стать");

            var complex = await _context.SportComplexes.FindAsync(dto.SportComplexId);
            if (complex == null) return BadRequest("Невідомий спорткомплекс");

            trainer.trainer_full_name = dto.TrainerFullName;
            trainer.trainer_phone_number = dto.TrainerPhoneNumber;
            trainer.trainer_gender_id = gender.gender_id;
            trainer.sport_complex_id = complex.sport_complex_id;

            var userName = Request.Headers["X-User-Name"].FirstOrDefault() ?? "Anonymous";
            var roleName = Request.Headers["X-User-Role"].FirstOrDefault() ?? "Unknown";
            LogService.LogAction(userName, roleName, $"Змінив дані тренера (ID: {trainerId})");
            await _context.SaveChangesAsync();
            return Ok("Тренера оновлено");
        }


        //SCHEDULE
        public class AddTrainerScheduleDto
        {
            public int TrainerId { get; set; }
            public string DayName { get; set; } = null!;
            public int ActivityId { get; set; }
            public string StartTime { get; set; } = null!;
            public string EndTime { get; set; } = null!;
        }

        [HttpPost("TrainerSchedules")]
        public async Task<IActionResult> AddTrainerSchedule([FromBody] AddTrainerScheduleDto dto)
        {
            var trainer = await _context.Trainers
                .Include(t => t.SportComplex)
                .FirstOrDefaultAsync(t => t.trainer_id == dto.TrainerId);

            if (trainer == null)
                return NotFound($"Тренера з ID {dto.TrainerId} не знайдено.");

            var dayOfWeek = await _context.DaysOfWeek.FirstOrDefaultAsync(d => d.day_name == dto.DayName);
            if (dayOfWeek == null)
                return BadRequest("Некоректний день тижня.");

            var gymIds = await _context.Gyms
                .Where(g => g.sport_complex_id == trainer.sport_complex_id)
                .Select(g => g.gym_id)
                .ToListAsync();

            if (!gymIds.Any())
                return BadRequest("У спорткомплексі тренера не знайдено жодного залу.");

            var activityInGym = await _context.ActivityInGyms
                .FirstOrDefaultAsync(aig =>
                    aig.activity_id == dto.ActivityId &&
                    gymIds.Contains(aig.gym_id));

            if (activityInGym == null)
                return BadRequest("Обрана активність недоступна в цьому спорткомплексі.");

            var newStartTime = TimeSpan.Parse(dto.StartTime);
            var newEndTime = TimeSpan.Parse(dto.EndTime);

            var existingSchedules = await _context.TrainerSchedules
                .Where(ts => ts.trainer_id == trainer.trainer_id)
                .Include(ts => ts.Schedule)
                .Where(ts => ts.Schedule.day_id == dayOfWeek.day_id)
                .ToListAsync();

            bool hasOverlap = existingSchedules.Any(ts =>
                !(newEndTime <= ts.Schedule.start_time || newStartTime >= ts.Schedule.end_time));

            if (hasOverlap)
                return BadRequest("У цей час тренер вже має інше заняття.");

            var schedule = new Schedule
            {
                day_id = dayOfWeek.day_id,
                start_time = newStartTime,
                end_time = newEndTime
            };
            _context.Schedules.Add(schedule);
            await _context.SaveChangesAsync();

            var trainerSchedule = new TrainerSchedule
            {
                trainer_id = trainer.trainer_id,
                schedule_id = schedule.schedule_id,
                activity_in_gym_id = activityInGym.activity_in_gym_id
            };
            _context.TrainerSchedules.Add(trainerSchedule);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Запис у розклад успішно додано!",
                ScheduleId = trainerSchedule.schedule_id
            });
        }

        [HttpDelete("TrainerSchedules/{scheduleId}")]
        public async Task<IActionResult> DeleteTrainerSchedule(int scheduleId)
        {
            var trainerSchedule = await _context.TrainerSchedules
                .Include(ts => ts.Schedule)
                .FirstOrDefaultAsync(ts => ts.schedule_id == scheduleId);

            if (trainerSchedule == null)
                return NotFound($"Запис з ID {scheduleId} не знайдено.");

            _context.Schedules.Remove(trainerSchedule.Schedule);
            _context.TrainerSchedules.Remove(trainerSchedule);

            await _context.SaveChangesAsync();
            return Ok(new { Message = "Запис успішно видалено!" });
        }


    }
}
