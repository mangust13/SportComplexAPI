using System.ComponentModel.DataAnnotations;

namespace SportComplexAPI.Models
{
    public class OrderStatus
    {
        [Key]
        public int order_status_id { get; set; }

        [Required]
        [MaxLength(50)]
        public string order_status_name { get; set; }

        public ICollection<Order> Orders { get; set; }
    }
}