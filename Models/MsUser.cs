using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace RentCar.Models
{
    public class MsUser : IdentityUser
    {
        [MaxLength(200)]
        public string FullName { get; set; } = string.Empty;
        [MaxLength(500)]
        public string Address { get; set; } = string.Empty;
    }
}
