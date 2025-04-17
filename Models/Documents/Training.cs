using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SportComplexAPI.Models
{
    public class Training
    {
        [Key]
        public int training_id { get; set; }

        public TimeSpan training_start_time { get; set; }

        public TimeSpan training_end_time { get; set; }

        [ForeignKey("TrainerSchedule")]
        public int trainer_schedule_id { get; set; }

        public TrainerSchedule TrainerSchedule { get; set; }
    }

}