using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SportComplexAPI.Models
{
    public class PurchasedProduct
    {
        [Key]
        public int purchased_product_id { get; set; }

        [ForeignKey("Order")]
        public int order_id { get; set; }

        [ForeignKey("Product")]
        public int product_id { get; set; }

        public int quantity { get; set; }

        public decimal unit_price { get; set; }

        public Order Order { get; set; }
        public Product Product { get; set; }
        public ICollection<Delivery> Deliveries { get; set; } = new List<Delivery>();
    }
}