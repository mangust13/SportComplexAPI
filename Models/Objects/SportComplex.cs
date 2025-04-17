using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SportComplexAPI.Models
{
    public class SportComplex
    {
        [Key]
        public int sport_complex_id { get; set; }
        [ForeignKey("City")]
        public int city_id { get; set; }
        public City City { get; set; }

        [Required]
        [MaxLength(255)]
        public string complex_address { get; set; }
    }
}
