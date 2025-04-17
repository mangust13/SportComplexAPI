using System.ComponentModel.DataAnnotations;

namespace SportComplexAPI.Models
{
    public class Brand
    {
        [Key]
        public int brand_id { get; set; }

        [Required]
        [MaxLength(255)]
        public string brand_name { get; set; }

        public ICollection<Product> Products { get; set; }
    }
}