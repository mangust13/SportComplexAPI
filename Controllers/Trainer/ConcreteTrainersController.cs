using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportComplexAPI.Data;

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
                .Include(t => t.TrainerSchedules)
                    .ThenInclude(ts => ts.ActivityInGym)
                        .ThenInclude(aig => aig.Activity)
                .Include(t => t.TrainerSchedules)
                    .ThenInclude(ts => ts.Schedule)
                        .ThenInclude(s => s.DayOfWeek)
                .Include(t => t.TrainerSchedules)
                    .ThenInclude(ts => ts.ActivityInGym)
                        .ThenInclude(aig => aig.Gym)
                            .ThenInclude(g => g.SportComplex)
                                .ThenInclude(sc => sc.City)
                .SingleOrDefaultAsync(t => t.trainer_id == trainerId);

            if (trainer == null)
                return NotFound("Trainer not found");

            var profileDto = new TrainerProfileDto
            {
                TrainerFullName = trainer.trainer_full_name,
                GymNumber = trainer.TrainerSchedules.FirstOrDefault()?.ActivityInGym.Gym.gym_number ?? 0,

                Activities = trainer.TrainerActivities
                    .Select(ta => new ActivityDto
                    {
                        ActivityId = ta.activity_id,
                        ActivityName = ta.Activity.activity_name,
                    })
                    .ToList(),

                Schedules = trainer.TrainerSchedules
                    .Select(ts => new TrainerScheduleEntryDto
                    {
                        ScheduleId = ts.schedule_id,
                        DayName = ts.Schedule.DayOfWeek.day_name,
                        StartTime = ts.Schedule.start_time.ToString(@"hh\:mm"),
                        EndTime = ts.Schedule.end_time.ToString(@"hh\:mm"),
                        ActivityName = ts.ActivityInGym.Activity.activity_name,
                        GymNumber = ts.ActivityInGym.Gym.gym_number,
                        SportComplexAddress = ts.ActivityInGym.Gym.SportComplex.complex_address,
                        SportComplexCity = ts.ActivityInGym.Gym.SportComplex.City.city_name
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
            public int ScheduleId { get; set; }
            public string DayName { get; set; } = null!;
            public string StartTime { get; set; } = null!;
            public string EndTime { get; set; } = null!;
            public string ActivityName { get; set; } = null!;
            public int GymNumber { get; set; }
            public string SportComplexAddress { get; set; } = null!;
            public string SportComplexCity { get; set; } = null!;
        }

    }
}
