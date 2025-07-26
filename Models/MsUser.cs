using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace RentCar.Models
{
    public class MsUser : IdentityUser
    {
        [Key]
        [MaxLength(200)]
        public string FullName { get; set; }
        [MaxLength(500)]
        public string Address { get; set; }
    }
}
