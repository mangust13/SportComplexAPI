using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SportComplexAPI.Models
{
    public class Delivery
    {
        [Key]
        public int delivery_id { get; set; }

        public DateTime delivery_date { get; set; }

        public int delivered_quantity { get; set; }

        [ForeignKey("PurchasedProduct")]
        public int purchased_product_id { get; set; }

        [ForeignKey("Inventory")]
        public int inventory_id { get; set; }

        public PurchasedProduct PurchasedProduct { get; set; }
        public Inventory Inventory { get; set; }
    }
}