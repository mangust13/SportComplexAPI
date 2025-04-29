using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SportComplexAPI.Models
{
    public class Trainer
    {
        [Key]
        public int trainer_id { get; set; }

        [ForeignKey("Gender")]
        public int trainer_gender_id { get; set; }
        public Gender Gender { get; set; }
        [ForeignKey("SportComplex")]
        public int sport_complex_id { get; set; }
        public SportComplex SportComplex { get; set; }

        [Required]
        [MaxLength(255)]
        public string trainer_full_name { get; set; }
        [MaxLength(15)]
        public string client_phone_number { get; set; }

        public ICollection<TrainerActivity> TrainerActivities { get; set; }
    }
}
