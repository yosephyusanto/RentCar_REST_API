using System.ComponentModel.DataAnnotations;

namespace RentCar.Models.Response
{
    public class MsCarResponse
    {
     
        public string Car_id { get; set; }
     
        public string Name { get; set; }
     
        public string Model { get; set; }
        public int Year { get; set; }
       
        public string License_plate { get; set; }
        public int Number_of_car_seats { get; set; }
      
        public string Transmission { get; set; }
        public decimal Price_per_day { get; set; }
        public bool Status { get; set; }

        public ICollection<MsCarImages> Images { get; set; }
    }
}
