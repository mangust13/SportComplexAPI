
using System.ComponentModel.DataAnnotations;

namespace SportComplexAPI.Models
{
    public class DayOfWeek
    {
        [Key]
        public int day_id { get; set; }
        [Required]
        [MaxLength(50)]
        public string day_name { get; set; }
        public ICollection<Schedule> Schedules { get; set; }
    }
}
