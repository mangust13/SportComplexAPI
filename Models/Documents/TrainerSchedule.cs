using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SportComplexAPI.Models
{
    public class TrainerSchedule
    {
        [Key]
        public int trainer_schedule_id { get; set; }

        [ForeignKey("Trainer")]
        public int trainer_id { get; set; }

        [ForeignKey("ActivityInGym")]
        public int activity_in_gym_id { get; set; }

        [ForeignKey("Schedule")]
        public int schedule_id { get; set; }

        public Trainer Trainer { get; set; }
        public ActivityInGym ActivityInGym { get; set; }
        public Schedule Schedule { get; set; }
    }
}