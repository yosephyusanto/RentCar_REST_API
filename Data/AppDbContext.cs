using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RentCar.Models;

namespace RentCar.Data
{
    public class AppDbContext : IdentityDbContext<MsUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }

        public DbSet<MsCar> MsCars { get; set; }
        public DbSet<MsCarImages> MsCarImages { get; set; }
        public DbSet<TrRental> TrRentals { get; set; }
        public DbSet<TrMaintenance> TrMaintenances { get; set; }
        public DbSet<LtPayment> LtPayments { get; set; }
    }
}
