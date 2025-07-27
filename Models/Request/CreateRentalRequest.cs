using System.ComponentModel.DataAnnotations;

namespace RentCar.Models.Request
{
    public class CreateRentalRequest
    {
        [Required]
        public string CarId { get; set; }

        [Required]
        public DateTime RentalDate { get; set; }

        [Required]
        public DateTime ReturnDate { get; set; }
    }
}
