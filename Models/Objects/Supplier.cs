using System.ComponentModel.DataAnnotations;

namespace SportComplexAPI.Models
{
    public class Supplier
    {
        [Key]
        public int supplier_id { get; set; }

        [Required]
        [MaxLength(255)]
        public string supplier_name { get; set; }

        [MaxLength(15)]
        public string supplier_phone_number { get; set; }

        [MaxLength(255)]
        public string supplier_license { get; set; }

        public ICollection<Order> Orders { get; set; }
    }
}