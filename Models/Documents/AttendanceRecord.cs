using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SportComplexAPI.Models
{
    public class AttendanceRecord
    {
        [Key]
        public int attendance_id { get; set; }

        [ForeignKey("Training")]
        public int training_id { get; set; }

        [ForeignKey("Purchase")]
        public int purchase_id { get; set; }

        public DateTime attendance_date_time { get; set; }

        public Training Training { get; set; }
        public Purchase Purchase { get; set; }
    }
}