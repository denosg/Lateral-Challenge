using Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Database
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        public DbSet<Shipment> Shipments => Set<Shipment>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Shipment>(ConfigureShipment);

            base.OnModelCreating(builder);
        }

        private void ConfigureShipment(EntityTypeBuilder<Shipment> entity)
        {
            entity.HasKey(x => x.ID);

            entity.Property(x => x.TrackingNumber)
                  .IsRequired()
                  .HasMaxLength(50);

            entity.HasIndex(x => x.TrackingNumber)
                  .IsUnique();

            entity.Property(x => x.Status)
                  .IsRequired();
            entity.HasIndex(x => x.Status);

            entity.Property(x => x.Version)
                  .IsConcurrencyToken()
                  .IsRequired();

            entity.Property(x => x.CreatedAt)
                  .HasDefaultValueSql("GETUTCDATE()");

            entity.HasIndex(x => new { x.Tenant, x.Status, x.IsDeleted })
                  .HasFilter("[IsDeleted] = 0");
        }
    }
}
