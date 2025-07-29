namespace RentCar.Models.Response
{
    public class RentalHistoryResponse
    {
        public string RentalId { get; set; }
        public string CarName { get; set; }
        public string CarModel { get; set; }
        public int CarYear { get; set; }
        public DateTime RentalDate { get; set; }
        public DateTime ReturnDate { get; set; }
        public int TotalDay { get; set; }
        public decimal TotalPrice { get; set; }
        public bool PaymentStatus { get; set; }
    }
}
