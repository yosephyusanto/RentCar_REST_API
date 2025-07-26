using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RentCar.Models
{
    [Table("TrRental")]
    public class TrRental
    {
        [Key]
        [MaxLength(36)]
        public string Rental_id { get; set; }
        public DateTime Rental_date { get; set; }
        public DateTime Return_date { get; set; }
        public decimal Total_price { get; set; }
        public bool Payment_status { get; set; }

        public string Customer_id { get; set; }
        [ForeignKey("Customer_id")]
        public MsUser User { get; set; }

        [MaxLength(36)]
        public string Car_id { get; set; }
        [ForeignKey("Car_id")]
        public MsCar Car { get; set; }
    }
}
