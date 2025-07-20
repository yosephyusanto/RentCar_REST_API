using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace RentCar.Models
{
    public class MsUser : IdentityUser
    {
        [Key]
        public string FullName { get; set; }
        public string Address { get; set; }
    }
}
