using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using RentCar.Data;
using RentCar.Models;
using RentCar.Models.Request;
using RentCar.Models.Response;

namespace RentCar.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TrRentalController : ControllerBase
    {
        private readonly AppDbContext _context;
        public TrRentalController(AppDbContext context)
        {
            _context = context;
        }

        [Authorize(Roles = "customer")]
        [HttpPost]
        public async Task<IActionResult> CreateRental([FromBody] CreateRentalRequest request )
        {
            try
            {
                // Ambil customer id dari Claim JWT
                var customerId = User.FindFirst("UserId")?.Value;
                if (string.IsNullOrEmpty(customerId)) 
                {
                    return Unauthorized("You are not logged in");
                }

                // Cek apakah mobil tersedia
                var car = await _context.MsCars.FindAsync(request.CarId);
                if (car == null) 
                {
                    return NotFound("Car not found");
                }

                if (!car.Status) // Status false = tidak tersedia
                {
                    return BadRequest("Car is not available for rent");
                }

                // Generate RentalId
                var latestRentalId = await _context.TrRentals
                    .OrderByDescending(x => x.Rental_id)
                    .Select(x => x.Rental_id)
                    .FirstOrDefaultAsync();

                int newRentalIdNumber;
                if(latestRentalId == null)
                {
                    newRentalIdNumber = 1;
                }
                else
                {
                    var lastNumber = int.Parse(latestRentalId.Substring(3));
                    newRentalIdNumber = lastNumber + 1;
                }
                var newRentalId = $"RNT{newRentalIdNumber:D3}";

                // total harga
                var rentalDays = (request.ReturnDate - request.RentalDate).Days;
                var totalPrice = rentalDays * car.Price_per_day;

                // Buat data rental
                var rental = new TrRental
                {
                    Rental_id = newRentalId,
                    Customer_id = customerId,
                    Car_id = request.CarId,
                    Rental_date = request.RentalDate,
                    Return_date = request.ReturnDate,
                    Total_price = totalPrice,
                    Payment_status = false // Belum dibayar
                };

                // Update status mobil menjadi tidak tersedia
                car.Status = false;

                _context.TrRentals.Add(rental);
                await _context.SaveChangesAsync();

                var response = new ApiResponse<object>
                {
                    StatusCode = StatusCodes.Status201Created,
                    RequestMethod = HttpContext.Request.Method,
                    Data = new
                    {
                        RentalId = newRentalId,
                        Message = "Rental created successfully",
                        TotalPrice = totalPrice
                    }
                };

                return Created($"/api/TrRental/{newRentalId}", response);
            }
            catch(Exception ex)
            {
                var errorResponse = new ApiResponse<string>
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    RequestMethod = HttpContext.Request.Method,
                    Data = ex.Message
                };
                return StatusCode(500, errorResponse);
            }
        }

        //[Authorize(Roles = "customer")]
        //[HttpGet("my-rentals")]
        //public async Task<ActionResult<IEnumerable<RentalHistoryResponse>>> GetMyRentals(
        //    [FromQuery] int page = 1,
        //    [FromQuery] int limit = 10)
        //{
        //    try
        //    {

        //    }
        //    catch (Exception ex)
        //    {

        //    }
        //}
    }
}
