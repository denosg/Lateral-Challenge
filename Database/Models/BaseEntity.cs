using System.ComponentModel.DataAnnotations;

namespace Infrastructure
{
    public class BaseEntity
    {
        [Key]
        public Guid ID { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public Guid? UpdatedBy { get; set; }
        public bool IsDeleted { get; set; }
        public Guid? DeletedBy { get; set; }
        public DateTime? DeletedAt { get; set; }

        [ConcurrencyCheck]
        public Guid Version { get; set; }
    }

    public class BaseTenantEntity : BaseEntity, IBaseTenantEntity
    {
        [Required, MaxLength(255)]
        public required string Tenant { get; set; }
    }

    public interface IAuditFields
    {
        DateTime CreatedAt { get; set; }
        Guid? CreatedBy { get; set; }
        DateTime? UpdatedAt { get; set; }
        Guid? UpdatedBy { get; set; }
        bool IsDeleted { get; set; }
        Guid? DeletedBy { get; set; }
    }

    public interface IBaseTenantEntity : IAuditFields
    {
        string Tenant { get; set; }
    }
}