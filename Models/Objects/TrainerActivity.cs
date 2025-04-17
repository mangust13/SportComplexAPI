using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SportComplexAPI.Models
{
    public class TrainerActivity
    {
        [Key]
        public int trainer_activity_id { get; set; }

        [ForeignKey("Trainer")]
        public int trainer_id { get; set; }

        [ForeignKey("Activity")]
        public int activity_id { get; set; }

        public Trainer Trainer { get; set; }
        public Activity Activity { get; set; }
    }
}