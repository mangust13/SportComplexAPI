using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SportComplexAPI.Models
{
    public class Product
    {
        [Key]
        public int product_id { get; set; }

        [ForeignKey("Brand")]
        public int brand_id { get; set; }

        [MaxLength(255)]
        public string product_model { get; set; }

        [ForeignKey("ProductType")]
        public int product_type_id { get; set; }

        [MaxLength(255)]
        public string product_description { get; set; }

        public Brand Brand { get; set; }
        public ProductType ProductType { get; set; }

        public ICollection<PurchasedProduct> PurchasedProducts { get; set; }
    }
}