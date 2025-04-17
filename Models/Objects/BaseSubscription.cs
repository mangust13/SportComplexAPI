using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SportComplexAPI.Models
{
    public class BaseSubscription
    {
        [Key]
        public int base_subscription_id { get; set; }

        [ForeignKey("SubscriptionTerm")]
        public int subscription_term_id { get; set; }

        [ForeignKey("SubscriptionVisitTime")]
        public int subscription_visit_time_id { get; set; }

        public SubscriptionTerm SubscriptionTerm { get; set; }
        public SubscriptionVisitTime SubscriptionVisitTime { get; set; }

        public ICollection<Subscription> Subscriptions { get; set; }
    }
}