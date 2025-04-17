using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SportComplexAPI.Models
{
    public class Schedule
    {
        [Key]
        public int schedule_id { get; set; }

        public TimeSpan start_time { get; set; }

        public TimeSpan end_time { get; set; }

        [ForeignKey("DayOfWeek")]
        public int day_id { get; set; }

        public DayOfWeek DayOfWeek { get; set; }

        public ICollection<TrainerSchedule> TrainerSchedules { get; set; }
    }
}