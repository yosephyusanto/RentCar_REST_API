namespace RentCar.Models.Response
{
    public class MsCarCardResponse
    {
        public string Car_id { get; set; }
        public string Name { get; set; }
        public decimal Price_per_day { get; set; }
        public bool Status { get; set; }
        public string? ImageLink { get; set; }
    }
}
