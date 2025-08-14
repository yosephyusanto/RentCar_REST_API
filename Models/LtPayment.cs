using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RentCar.Models
{
    public class LtPayment
    {
        [Key]
        [MaxLength(36)]
        public string Payment_Id { get; set; } = string.Empty;
        public DateTime Payment_date { get; set; }
        [Column(TypeName = "numeric(18,2)")]
        public decimal Amount { get; set; }
        [MaxLength(100)]
        public string Payment_method { get; set; } = string.Empty;

        public string Rental_id { get; set; } = string.Empty;
        [ForeignKey("Rental_id")]
        public TrRental Rental { get; set; } = new TrRental();

    }
}
