using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SportComplexAPI.Models
{
    public class Inventory
    {
        [Key]
        public int inventory_id { get; set; }

        [ForeignKey("Gym")]
        public int gym_id { get; set; }

        [ForeignKey("Product")]
        public int product_id { get; set; }

        public int quantity { get; set; }

        public Gym Gym { get; set; }
        public Product Product { get; set; }
    }
}