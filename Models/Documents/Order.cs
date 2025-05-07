using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SportComplexAPI.Models
{
    public class Order
    {
        [Key]
        public int order_id { get; set; }

        [ForeignKey("Supplier")]
        public int supplier_id { get; set; }

        public DateTime order_date { get; set; }

        public decimal order_total_price { get; set; }

        [ForeignKey("OrderStatus")]
        public int order_status_id { get; set; }

        [ForeignKey("PaymentMethod")]
        public int payment_method_id { get; set; }

        public int order_number { get; set; }

        public Supplier Supplier { get; set; }
        public OrderStatus OrderStatus { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public ICollection<PurchasedProduct> PurchasedProducts { get; set; } = new List<PurchasedProduct>();
    }
}