using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SportComplexAPI.Models
{
    public class Subscription
    {
        [Key]
        public int subscription_id { get; set; }

        [ForeignKey("BaseSubscription")]
        public int base_subscription_id { get; set; }

        [MaxLength(255)]
        public string subscription_name { get; set; }

        public decimal subscription_total_cost { get; set; }

        public BaseSubscription BaseSubscription { get; set; }
        public ICollection<SubscriptionActivity> SubscriptionActivities { get; set; }
        public ICollection<Purchase> Purchases { get; set; }
    }
}