using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;
using RentCar.Data;
using RentCar.Models.Response;
using RentCar.Models;
using RouteAttribute = Microsoft.AspNetCore.Mvc.RouteAttribute;
using Microsoft.EntityFrameworkCore;
using RentCar.Models.Request;
using Microsoft.AspNetCore.Authorization;

namespace RentCar.Controllers
{
    [Authorize(Roles = "employee")]
    [Route("api/[controller]")]
    [ApiController]
    public class MsCarController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;  
        public MsCarController(AppDbContext context, IWebHostEnvironment hostEnvironment) 
        { 
            _context = context;
            _hostEnvironment = hostEnvironment;
        }

        [AllowAnonymous] 
        [Authorize(Roles = "customer,employee")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MsCarCardResponse>>> Get() 
        {
            try
            {
                var listCars = await _context.MsCars
               .Include(x => x.Images)
               .Select(
                   x => new MsCarCardResponse
                   {
                       Car_id = x.Car_id,
                       Name = x.Name,
                       Price_per_day = x.Price_per_day,
                       Status = x.Status,
                       ImageLink = x.Images.FirstOrDefault().Image_link ?? null,
                   }
               ).ToListAsync();

                var response = new ApiResponse<IEnumerable<MsCarCardResponse>>
                {
                    StatusCode = StatusCodes.Status200OK,
                    RequestMethod = HttpContext.Request.Method,
                    Data = listCars,
                };

                return Ok(response);
            }
            catch(Exception ex)
            {
                var response = new ApiResponse<string>
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    RequestMethod = HttpContext.Request.Method,
                    Data = ex.Message,
                };
                return StatusCode(500, response);
            }
           
        }

        [AllowAnonymous]
        [Authorize(Roles = "customer,employee")]
        [HttpGet("{carId}")]
        public async Task<ActionResult<MsCarResponse>> GetById(string carId)
        {
            try
            {
                var car = await _context.MsCars
                    .Include(x => x.Images)
                    .Where(x => x.Car_id == carId)
                    .Select(x => new MsCarResponse
                    {
                        Car_id = x.Car_id,
                        Name = x.Name,
                        Model = x.Model,
                        Year = x.Year,
                        License_plate = x.License_plate,
                        Number_of_car_seats = x.Number_of_car_seats,
                        Transmission = x.Transmission,
                        Price_per_day = x.Price_per_day,
                        Status = x.Status,
                        Images = x.Images,
                    }).FirstOrDefaultAsync();

                if( car == null)
                {
                    var notFoundResponse = new ApiResponse<string>
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        RequestMethod = HttpContext.Request.Method,
                        Data = "Car not found"
                    };

                    return NotFound(notFoundResponse);
                }

                var response = new ApiResponse<MsCarResponse>
                {
                    StatusCode = StatusCodes.Status200OK,
                    RequestMethod = HttpContext.Request.Method,
                    Data = car,
                };

                return Ok(response);
            }
            catch(Exception ex)
            {
                var response = new ApiResponse<string>
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    RequestMethod = HttpContext.Request.Method,
                    Data = ex.Message,
                };
                return StatusCode(500, response);
            }
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Post([FromBody] CreateMsCarRequest request)
        {
            try
            {
                if(request == null)
                {
                    var badResponse = new ApiResponse<string>
                    {
                        StatusCode = StatusCodes.Status200OK,
                        RequestMethod = HttpContext.Request.Method,
                        Data = "Invalid request data",
                    };
                    return BadRequest(badResponse);
                }

                var latestCarId = await _context.MsCars
                    .OrderByDescending(x => x.Car_id)
                    .Select(x => x.Car_id)
                    .FirstOrDefaultAsync();

                int newCarIdNumber;
                if(latestCarId == null)
                {
                    newCarIdNumber = 1;
                }
                else
                {
                    var lastNumber = int.Parse(latestCarId.Substring(3));
                    newCarIdNumber = lastNumber + 1;
                }

                var newCarId = $"CAR{newCarIdNumber:D3}";

                var carData = new MsCar
                {
                    Car_id = newCarId,
                    Name = request.Name,
                    Model = request.Model,
                    Year = request.Year,
                    License_plate = request.License_plate,
                    Number_of_car_seats = request.Number_of_car_seats,
                    Transmission = request.Transmission,
                    Price_per_day = request.Price_per_day,
                    Status = false,
                };

                _context.MsCars.Add(carData);   
                await _context.SaveChangesAsync();

                var response = new ApiResponse<string> { 
                    StatusCode = StatusCodes.Status201Created,
                    RequestMethod = HttpContext.Request.Method,
                    Data = "Car data saved  successfully"
                };

                return Created($"/api/MsCar/{newCarId}", response);
            }
            catch (Exception ex)
            {
                var response = new ApiResponse<string>
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    RequestMethod = HttpContext.Request.Method,
                    Data = ex.Message,
                };
                return StatusCode(500, response);
            }
        }


        [HttpPost("upload-images/{carId}")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadImages(string carId, IFormFile[] files)
        {
            if (files == null || files.Length == 0)
            {
                var badResponse = new ApiResponse<string> { 
                    StatusCode = StatusCodes.Status400BadRequest,
                    RequestMethod = HttpContext.Request.Method,
                    Data = "No files uploaded" 
                };

                return BadRequest(badResponse);
            }

            try
            {
                var carExists = await _context.MsCars.AnyAsync(x => x.Car_id == carId);
                if (!carExists)
                {
                    var notFoundResponse = new ApiResponse<string> { 
                        StatusCode = StatusCodes.Status400BadRequest,
                        RequestMethod = HttpContext.Request.Method,
                        Data = "Car not found"
                    };

                    return NotFound(notFoundResponse);
                }

                // Buat folder penyimpanan images jika belum ada
                var uploadPath = Path.Combine(_hostEnvironment.WebRootPath, "images", "cars");
                if (!Directory.Exists(uploadPath))
                {
                    Directory.CreateDirectory(uploadPath);
                }


                // supaya efisien cukup fetch id sekali saja jangan di dalam looping
                var latestImageId = await _context.MsCarImages
                    .OrderByDescending(x => x.Image_car_id)
                    .Select(x => x.Image_car_id)
                    .FirstOrDefaultAsync();

                int newImageIdNumber;
                if (latestImageId == null)
                {
                    newImageIdNumber = 1;
                }
                else
                {
                    var lastNumber = int.Parse(latestImageId.Substring(3));
                    newImageIdNumber = lastNumber + 1;
                }

                var imageUrls = new List<string>();

                foreach (var file in files)
                {
                    if(file.Length > 0)
                    {
                        // validasi ekstensi file
                        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
                        var fileExtensions = Path.GetExtension(file.FileName).ToLower();
                        if (!allowedExtensions.Contains(fileExtensions))
                        {
                            var invalidFileResponse = new ApiResponse<string>
                            {
                                StatusCode = StatusCodes.Status400BadRequest,
                                RequestMethod = HttpContext.Request.Method,
                                Data = $"Invalid file type {file.FileName}. Only image files are allowed"
                            };
                            return BadRequest(invalidFileResponse);
                        }

                        // validasi ukuran file (maksimal 5MB)
                        if(file.Length > 5 * 1024 * 1024)
                        {
                            var fileSizeResponse = new ApiResponse<string>
                            {
                                StatusCode = StatusCodes.Status400BadRequest,
                                RequestMethod = HttpContext.Request.Method,
                                Data = $"File {file.FileName} is too large. Maximum size is 5MB"
                            };
                            return BadRequest(fileSizeResponse);
                        }

                        var fileName = Guid.NewGuid().ToString() + fileExtensions;
                        var filePath = Path.Combine(uploadPath, fileName);
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }

                        var newImageId = $"IMG{newImageIdNumber:D3}";
                        newImageIdNumber++;

                        var imageData = new MsCarImages
                        {
                            Car_id = carId,
                            Image_car_id = newImageId,
                            Image_link = $"/images/cars/{fileName}"
                        };

                        _context.MsCarImages.Add(imageData);
                        imageUrls.Add(imageData.Image_link);
                    }
                }

                await _context.SaveChangesAsync();

                var response = new ApiResponse<object>
                {
                    StatusCode = StatusCodes.Status200OK,
                    RequestMethod = HttpContext.Request.Method,
                    Data = new
                    {
                        Message = "Image uploaded successfully",
                        CarId = carId,
                        ImageUrls = imageUrls,
                    }
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                var response = new ApiResponse<string>
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    RequestMethod = HttpContext.Request.Method,
                    Data = ex.Message,
                };
                return BadRequest(response);
            }
        }
    }
}
