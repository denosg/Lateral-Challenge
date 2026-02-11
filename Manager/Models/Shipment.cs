using Infrastructure;

namespace Manager.Models
{
    public class Shipment
    {
        public Guid ID { get; set; }

        public DateTime CreatedAt { get; set; }

        public Guid? CreatedBy { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public Guid? UpdatedBy { get; set; }

        public bool IsDeleted { get; set; }

        public Guid? DeletedBy { get; set; }

        public DateTime? DeletedAt { get; set; }

        public string TrackingNumber { get; set; }

        public string RecipientName { get; set; }

        public ShipmentStatus Status { get; set; }

        public Guid Version { get; set; }
    }
}
