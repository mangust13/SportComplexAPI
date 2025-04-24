using System.ComponentModel.DataAnnotations;

namespace SportComplexAPI.Models
{
    public class Gender
    {
        [Key]
        public int gender_id { get; set; }
        [Required]
        [MaxLength(50)]
        public string gender_name { get; set; }

        public ICollection<Client> Clients { get; set; }
        public ICollection<Trainer> Trainers { get; set; }
        
    }
}
