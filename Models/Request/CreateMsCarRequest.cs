using System.ComponentModel.DataAnnotations;

namespace RentCar.Models.Request
{
    public class CreateMsCarRequest
    {
        [MaxLength(200)]
        public string Name { get; set; }
        [MaxLength(100)]
        public string Model { get; set; }
        public int Year { get; set; }
        [MaxLength(50)]
        public string License_plate { get; set; }
        public int Number_of_car_seats { get; set; }
        [MaxLength(100)]
        public string Transmission { get; set; }
        public decimal Price_per_day { get; set; }
    }
}
