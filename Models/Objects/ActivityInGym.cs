using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SportComplexAPI.Models
{
    public class ActivityInGym
    {
        [Key]
        public int activity_in_gym_id { get; set; }

        [ForeignKey("Activity")]
        public int activity_id { get; set; }

        [ForeignKey("Gym")]
        public int gym_id { get; set; }

        public Activity Activity { get; set; }
        public Gym Gym { get; set; }

        public ICollection<TrainerSchedule> TrainerSchedules { get; set; }
    }
}