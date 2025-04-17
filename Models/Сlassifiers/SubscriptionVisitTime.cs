using System.ComponentModel.DataAnnotations;

namespace SportComplexAPI.Models
{
    public class SubscriptionVisitTime
    {
        [Key]
        public int subscription_visit_time_id { get; set; }

        [Required]
        [MaxLength(255)]
        public string subscription_visit_time { get; set; }

        public ICollection<BaseSubscription> BaseSubscriptions { get; set; }
    }
}