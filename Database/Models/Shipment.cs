using Infrastructure;
using System.ComponentModel.DataAnnotations;

namespace Database.Models
{
    public class Shipment: BaseTenantEntity
    {
        [Required, MaxLength(255)]
        public string TrackingNumber { get; set; }

        [Required, MaxLength(255)]
        public string RecipientName { get; set; }

        [Required]
        public ShipmentStatus Status { get; set; }
    }
}
