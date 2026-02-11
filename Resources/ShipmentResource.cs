using AutoMapper;
using Database;
using Database.Models;
using Infrastructure;
using Infrastructure.Services;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Resources
{
    public interface IShipmentResource
    {
        Task<Models.Shipment?> GetByIdAsync(Guid id);
        Task<Guid> SaveAsync(Models.Shipment shipment);
        Task DeleteAsync(Guid id, Guid deletedBy);
        Task<IList<Models.Shipment>> GetByStatusAsync(ShipmentStatus status);
        Task<IList<Models.Shipment>> GetAllAsync(int page = 1, int pageSize = 50);
    }

    public class ShipmentResource(
        AppDbContext db,
        IMapper mapper,
        ILogger<ShipmentResource> logger,
        ITenantGetter tenantGetter) : IShipmentResource
    {
        private readonly AppDbContext _db = db;
        private readonly IMapper _mapper = mapper;
        private readonly ITenantGetter _tenantGetter = tenantGetter;

        public async Task<Models.Shipment?> GetByIdAsync(Guid id)
        {
            var entity = await _db.Shipments
                .Where(s => s.ID == id && !s.IsDeleted && s.Tenant == _tenantGetter.Tenant)
                .SingleOrDefaultAsync();

            return _mapper.Map<Models.Shipment?>(entity);
        }

        public async Task<IList<Models.Shipment>> GetAllAsync(int page = 1, int pageSize = 50)
        {
            var entities = await _db.Shipments
                .Where(s => !s.IsDeleted && s.Tenant == _tenantGetter.Tenant)
                .OrderByDescending(s => s.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return _mapper.Map<IList<Models.Shipment>>(entities);
        }

        public async Task<IList<Models.Shipment>> GetByStatusAsync(ShipmentStatus status)
        {
            var entities = await _db.Shipments
                .Where(s => s.Status == status && !s.IsDeleted && s.Tenant == _tenantGetter.Tenant)
                .OrderBy(s => s.CreatedAt)
                .ToListAsync();

            return _mapper.Map<IList<Models.Shipment>>(entities);
        }

        public async Task<Guid> SaveAsync(Models.Shipment shipment)
        {
            bool isNew = shipment.ID == Guid.Empty;

            if (isNew)
            {
                shipment.ID = NewId.NextSequentialGuid();
                var dbEntity = _mapper.Map<Shipment>(shipment);

                dbEntity.CreatedAt = DateTime.UtcNow;
                dbEntity.Version = Guid.NewGuid();
                dbEntity.IsDeleted = false;
                dbEntity.Tenant = _tenantGetter.Tenant;

                _db.Shipments.Add(dbEntity);
            }
            else
            {
                var existing = await _db.Shipments
                    .Where(s => s.ID == shipment.ID && !s.IsDeleted && s.Tenant == _tenantGetter.Tenant)
                    .SingleOrDefaultAsync();

                if (existing == null)
                {
                    throw new InvalidOperationException($"Shipment with ID {shipment.ID} not found");
                }

                if (existing.Version != shipment.Version)
                {
                    throw new DbUpdateConcurrencyException("The entity was modified before you could update it");
                }
                var oldVersion = shipment.Version;

                _mapper.Map(shipment, existing);

                existing.UpdatedAt = DateTime.UtcNow;
                existing.Tenant = _tenantGetter.Tenant;

                _db.Shipments.Update(existing);
                _db.Shipments.Entry(existing).Property(p => p.Version).OriginalValue = oldVersion;

                // Set Version AFTER Update() and OriginalValue
                existing.Version = NewId.NextSequentialGuid();
                _db.Shipments.Entry(existing).Property(p => p.Version).IsModified = true;
            }

            try
            {
                await _db.SaveChangesAsync();
                // Clear change tracker to prevent concurrency issues when saving multiple related entities
                _db.ChangeTracker.Clear();

                return shipment.ID;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                logger.LogWarning(ex, "Concurrency conflict for shipment {ShipmentId}", shipment.ID);
                throw;
            }
            catch (DbUpdateException ex)
            {
                logger.LogError(ex, "Database error saving shipment {ShipmentId}", shipment.ID);
                throw new ShipmentDataAccessException("Failed to save shipment to database", ex);
            }
        }

        public async Task DeleteAsync(Guid id, Guid deletedBy)
        {
            await _db.Shipments
                .Where(s => s.ID == id && !s.IsDeleted && s.Tenant == _tenantGetter.Tenant)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(s => s.IsDeleted, true)
                    .SetProperty(s => s.DeletedAt, DateTime.UtcNow)
                    .SetProperty(s => s.DeletedBy, deletedBy));
        }
    }
}