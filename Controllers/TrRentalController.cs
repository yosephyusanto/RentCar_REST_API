using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
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
                        TotalDay = (r.Return_date - r.Rental_date).Days,
                        PricePerDay = r.Car.Price_per_day, 
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

        // payment endpoint
        [Authorize(Roles = "customer")]
        [HttpPost("{rentalId}/payment")]
        public async Task<IActionResult> ProcessPayment(string rentalId, [FromBody] string paymentMethod)
        {
            try
            {
                var customerId = User.FindFirst("UserId")?.Value;
                if (string.IsNullOrEmpty(customerId))
                {
                    return Unauthorized("Customer not found");
                }

                var rental = await _context.TrRentals
                    .FirstOrDefaultAsync(r => r.Rental_id == rentalId && r.Customer_id == customerId);

                if(rental == null)
                {
                    return NotFound("rental not found");
                }
                if (rental.Payment_status)
                {
                    return BadRequest("Rental already paid");
                }

                var latestPaymentId = _context.LtPayments
                    .OrderByDescending(p => p.Payment_Id)
                    .Select(p => p.Payment_Id)
                    .FirstOrDefault();

                int newPaymentIdNumber;

                if (latestPaymentId == null)
                {
                    newPaymentIdNumber = 1;
                }
                else
                {
                    var lastNumber = int.Parse(latestPaymentId.Substring(3));
                    newPaymentIdNumber = lastNumber + 1;
                }

                var newPaymentId = $"PY{newPaymentIdNumber:D3}";

                decimal totalPrice = await _context.TrRentals
                    .Where(r => r.Rental_id == rentalId)
                    .Select(r => r.Total_price)
                    .FirstOrDefaultAsync();

                var newPayment = new LtPayment
                {
                    Payment_Id = newPaymentId,
                    Rental_id = rentalId,
                    Amount = totalPrice,
                    Payment_date = DateTime.Now,
                    Payment_method = paymentMethod,
                };

                _context.LtPayments.Add(newPayment);

                var response = new ApiResponse<string>
                {
                    StatusCode = StatusCodes.Status200OK,
                    RequestMethod = HttpContext.Request.Method,
                    Data = "Success Payment"
                };

                rental.Payment_status = true;

                return Ok(response);
            }
            catch (Exception ex)
            {
                var errorResponse = new ApiResponse<string>
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    RequestMethod = HttpContext.Request.Method,
                    Data = ex.Message,
                };
                return StatusCode(500, errorResponse);
            }
        }
    }
}
