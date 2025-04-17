using System.ComponentModel.DataAnnotations;

namespace SportComplexAPI.Models
{
    public class GymStatus
    {
        [Key]
        public int gym_status_id { get; set; }
        [Required]
        [MaxLength(50)]
        public string gym_status_name { get; set; }

        public ICollection<Gym> Gyms { get; set; }
    }
}
