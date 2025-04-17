using System.ComponentModel.DataAnnotations;

namespace SportComplexAPI.Models
{
    public class City
    {
        [Key]
        public int city_id { get; set; }
        [Required]
        [MaxLength(255)]
        public string city_name { get; set; }
        public ICollection<SportComplex> SportComplexes { get; set; }
    }
}
