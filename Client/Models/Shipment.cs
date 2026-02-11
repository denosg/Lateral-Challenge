using Infrastructure;
using System.ComponentModel.DataAnnotations;

namespace Client.Models
{
    public class Shipment
    {
        public Guid ID { get; set; }

        public DateTime CreatedAt { get; set; }
        public Guid Version { get; set; }

        [Required(ErrorMessage = "Tracking number is required")]
        [MaxLength(50, ErrorMessage = "Tracking number cannot exceed 50 characters")]
        public string TrackingNumber { get; set; }

        [Required(ErrorMessage = "Recipient name is required")]
        [MaxLength(255, ErrorMessage = "Recipient name cannot exceed 255 characters")]
        public string RecipientName { get; set; }

        [Required]
        public ShipmentStatus Status { get; set; }
    }

    public class ShipmentPost
    {
        [Required(ErrorMessage = "Tracking number is required")]
        [MaxLength(50, ErrorMessage = "Tracking number cannot exceed 50 characters")]
        public string TrackingNumber { get; set; }

        [Required(ErrorMessage = "Recipient name is required")]
        [MaxLength(255, ErrorMessage = "Recipient name cannot exceed 255 characters")]
        public string RecipientName { get; set; }
    }

    public class ShipmentPut
    {
        [Required(ErrorMessage = "Tracking number is required")]
        [MaxLength(50, ErrorMessage = "Tracking number cannot exceed 50 characters")]
        public string TrackingNumber { get; set; }

        [Required(ErrorMessage = "Recipient name is required")]
        [MaxLength(255, ErrorMessage = "Recipient name cannot exceed 255 characters")]
        public string RecipientName { get; set; }

        public Guid Version { get; set; }
    }

    public class UpdateStatusRequest
    {
        [Required]
        public ShipmentStatus Status { get; set; }

        [Required]
        public Guid Version { get; set; }
    }
}
