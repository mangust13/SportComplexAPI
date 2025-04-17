using System.ComponentModel.DataAnnotations;

namespace SportComplexAPI.Models
{
    public class ProductType
    {
        [Key]
        public int product_type_id { get; set; }

        [Required]
        [MaxLength(255)]
        public string product_type_name { get; set; }

        public ICollection<Product> Products { get; set; }
    }
}