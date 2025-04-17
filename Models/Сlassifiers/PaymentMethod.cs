using System.ComponentModel.DataAnnotations;

namespace SportComplexAPI.Models
{
    public class PaymentMethod
    {
        [Key]
        public int payment_method_id { get; set; }

        [Required]
        [MaxLength(255)]
        public string payment_method { get; set; }

        public ICollection<Purchase> Purchases { get; set; }
        public ICollection<Order> Orders { get; set; }
    }
}