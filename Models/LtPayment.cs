using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RentCar.Models
{
    public class LtPayment
    {
        [Key]
        [MaxLength(36)]
        public string Payment_Id { get; set; }
        public DateTime Payment_date { get; set; }
        public decimal Amount { get; set; }
        [MaxLength(100)]
        public string Payment_method { get; set; }

        public string Rental_id { get; set; }
        [ForeignKey("Rental_id")]
        public TrRental Rental { get; set; }

    }
}
