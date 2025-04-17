using System.ComponentModel.DataAnnotations;

namespace SportComplexAPI.Models
{
    public class Activity
    {
        [Key]
        public int activity_id { get; set; }

        [Required]
        [MaxLength(255)]
        public string activity_name { get; set; }

        [MaxLength(255)]
        public string activity_description { get; set; }

        public decimal activity_price { get; set; }

        public ICollection<ActivityInGym> ActivityInGyms { get; set; }
        public ICollection<SubscriptionActivity> SubscriptionActivities { get; set; }
        public ICollection<TrainerActivity> TrainerActivities { get; set; }
    }
}