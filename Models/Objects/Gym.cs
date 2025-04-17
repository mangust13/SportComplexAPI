using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SportComplexAPI.Models
{
    public class Gym
    {
        [Key]
        public int gym_id { get; set; }
        [ForeignKey("GymStatus")]
        public int gym_status_id { get; set; }
        public GymStatus GymStatus { get; set; }
        [ForeignKey("SportComplex")]
        public int sport_complex_id { get; set; }
        public SportComplex SportComplex { get; set; }
    }
}
