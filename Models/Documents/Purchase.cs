using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SportComplexAPI.Models
{
    public class Purchase
    {
        [Key]
        public int purchase_id { get; set; }

        [ForeignKey("Client")]
        public int client_id { get; set; }
        public Client Client { get; set; }


        [ForeignKey("Subscription")]
        public int subscription_id { get; set; }

        [ForeignKey("PaymentMethod")]
        public int payment_method_id { get; set; }

        public int purchase_number { get; set; }

        public DateTime purchase_date { get; set; }

        public Subscription Subscription { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public ICollection<AttendanceRecord> AttendanceRecords { get; set; }

    }

}