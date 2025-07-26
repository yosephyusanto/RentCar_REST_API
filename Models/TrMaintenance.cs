using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RentCar.Models
{
    [Table("TrMaintenance")]
    public class TrMaintenance
    {
        [Key]
        [MaxLength(36)]
        public string Maintenance_Id { get; set; }
        public DateTime Maintanance_date { get; set; }
        [MaxLength(4000)]
        public string Description { get; set; }
        public decimal Cost { get; set; }

        public string Car_id { get; set; }
        [ForeignKey("Car_id")]
        public MsCar Car { get; set; }

        public string Employee_id { get; set; }
        [ForeignKey("Employee_id")]
        public MsUser User { get; set; }
    }
}
