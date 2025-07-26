using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RentCar.Models
{
    [Table("MsCar")]
    public class MsCar
    {
        [Key]
        [MaxLength(36)]
        public string Car_id { get; set; }
        [MaxLength(200)]
        public string Name { get; set; }
        [MaxLength(100)]
        public string Model { get; set; }
        public int Year { get; set; }
        [MaxLength(50)]
        public string License_plate { get; set; } 
        public int Number_of_car_seats { get; set; }
        [MaxLength(100)]
        public string Transmission {  get; set; }
        public decimal Price_per_day { get; set; }
        public bool  Status { get; set; }

        // Navigational Properties
        public ICollection<MsCarImages> Images { get; set; } 
        public ICollection<TrRental> Rentals { get; set; } = new List<TrRental>();
        public ICollection<TrMaintenance> Maintenances { get; set; } = new List<TrMaintenance>();

    }
}
