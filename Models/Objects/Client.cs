using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SportComplexAPI.Models
{
    public class Client
    {
        [Key]
        public int client_id { get; set; }
        public int client_gender_id { get; set; }
        [ForeignKey("client_gender_id")]
        public Gender Gender { get; set; }
              
        [Required]
        [MaxLength(255)]
        public string client_full_name { get; set; }
        [MaxLength(15)]
        public string client_phone_number { get; set; }
    }
}
