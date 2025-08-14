using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RentCar.Models
{
    [Table("TrRental")]
    public class TrRental
    {
        [Key]
        [MaxLength(36)]
        public string Rental_id { get; set; } = string.Empty;
        public DateTime Rental_date { get; set; }
        public DateTime Return_date { get; set; }
        [Column(TypeName = "numeric(18,2)")]
        public decimal Total_price { get; set; }
        public bool Payment_status { get; set; }

        public string Customer_id { get; set; } = string.Empty;
        [ForeignKey("Customer_id")]
        public MsUser User { get; set; } = new MsUser();

        [MaxLength(36)]
        public string Car_id { get; set; } = string.Empty;
        [ForeignKey("Car_id")]
        public MsCar Car { get; set; } = new MsCar();
    }
}
