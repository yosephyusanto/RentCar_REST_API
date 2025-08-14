using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RentCar.Models
{
    [Table("TrMaintenance")]
    public class TrMaintenance
    {
        [Key]
        [MaxLength(36)]
        public string Maintenance_Id { get; set; } = string.Empty;
        public DateTime Maintanance_date { get; set; }
        [MaxLength(4000)]
        public string Description { get; set; } = string.Empty;
        public decimal Cost { get; set; }

        public string Car_id { get; set; } = string.Empty;
        [ForeignKey("Car_id")]
        public MsCar Car { get; set; } = new MsCar();

        public string Employee_id { get; set; } = string.Empty;
        [ForeignKey("Employee_id")]
        public MsUser User { get; set; } = new MsUser();
    }
}
