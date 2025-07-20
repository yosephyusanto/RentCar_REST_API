namespace RentCar.Models.Response
{
    public class PaginatedResponse<T>
    {
        public int StatusCode { get; set; }
        public string RequestMethod { get; set; }
        public T Data { get; set; }
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }
    }
}
