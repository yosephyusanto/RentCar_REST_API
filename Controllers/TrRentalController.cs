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

        [Authorize(Roles = "customer")]
        [HttpGet("my-rentals")]
        public async Task<ActionResult<IEnumerable<RentalHistoryResponse>>> GetMyRentals(
            [FromQuery] int page = 1,
            [FromQuery] int limit = 10)
        {
            try
            {
                var customerId = User.FindFirst("UserId")?.Value;
                if (string.IsNullOrEmpty(customerId))
                {
                    return Unauthorized("Customer not found");
                }

                // eager loading or join  
                var query = _context.TrRentals
                    .Include(r => r.Car)
                    .Where(r => r.Customer_id == customerId)
                    .OrderByDescending(r => r.Rental_date);

                var totalItems = await query.CountAsync();

                // query already contain all join data so, no need to using async because this command will not be a query to database
                var rentals = query
                    .Skip((page - 1) * limit)
                    .Take(limit)
                    .Select(r => new RentalHistoryResponse
                    {
                        RentalId = r.Rental_id,
                        CarName = r.Car.Name,
                        CarModel = r.Car.Model,
                        CarYear = r.Car.Year,
                        RentalDate = r.Rental_date,
                        ReturnDate = r.Return_date,
                        TotalDay = (r.Return_date.Day - r.Rental_date.Day),
                        TotalPrice = r.Total_price,
                        PaymentStatus = r.Payment_status,
                    });

                var totalPages = (int)Math.Ceiling(totalItems / (double)limit);

                var response = new PaginatedResponse<IEnumerable<RentalHistoryResponse>>
                {
                    StatusCode = StatusCodes.Status200OK,
                    RequestMethod = HttpContext.Request.Method,
                    Data = rentals,
                    TotalItems = totalItems,
                    TotalPages = totalPages,
                    CurrentPage = page,
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                var errorResponse = new PaginatedResponse<string>
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    RequestMethod = HttpContext.Request.Method,
                    Data = ex.Message,
                    TotalItems = 0,
                    TotalPages = 0,
                    CurrentPage = page
                };
                return StatusCode(500, errorResponse);
            }
        }
    }
}
