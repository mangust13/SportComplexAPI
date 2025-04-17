using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SportComplexAPI.Models
{
    public class SubscriptionActivity
    {
        [Key]
        public int subscription_activity_id { get; set; }

        [ForeignKey("Subscription")]
        public int subscription_id { get; set; }

        [ForeignKey("Activity")]
        public int activity_id { get; set; }

        public int activity_type_amount { get; set; }

        public Subscription Subscription { get; set; }
        public Activity Activity { get; set; }
    }
}