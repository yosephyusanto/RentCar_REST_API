using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RentCar.Models
{
    [Table("MsCarImages")]
    public class MsCarImages
    {
        [Key]
        [MaxLength(36)]
        public string Image_car_id { get; set; } = string.Empty;

        [MaxLength(2000)]
        public string Image_link { get; set; } = string.Empty;
        public string PublicId { get; set; } = string.Empty;

        [MaxLength(36)]
        public string Car_id { get; set; } = string.Empty;

        [ForeignKey("Car_id")]
        public MsCar Car { get; set; } = new MsCar();

    }
}
