using AutoMapper;
using Infrastructure;
using Manager.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace Manager
{
    public interface IShipmentManager
    {
        Task<Shipment?> GetShipmentByIdAsync(Guid id);
        Task<IList<Shipment>> GetAllShipmentsAsync(int page = 1, int pageSize = 50);
        Task<IList<Shipment>> GetShipmentsByStatusAsync(ShipmentStatus status);
        Task<Guid> CreateShipmentAsync(Shipment shipment, Guid? createdBy);
        Task<Shipment?> UpdateShipmentAsync(Shipment shipment, Guid? updatedBy);
        Task DeleteShipmentAsync(Guid id, Guid deletedBy);
        Task<Shipment?> UpdateShipmentStatusAsync(Guid id, ShipmentStatus newStatus, Guid? updatedBy);
    }

    public class ShipmentManager(
        Resources.IShipmentResource resource,
        ILogger<ShipmentManager> logger,
        IMapper mapper) : IShipmentManager
    {
        public async Task<Shipment?> GetShipmentByIdAsync(Guid id)
        {
            var entity = await resource.GetByIdAsync(id);
            return mapper.Map<Shipment?>(entity);
        }

        public async Task<IList<Shipment>> GetAllShipmentsAsync(int page = 1, int pageSize = 50)
        {
            var entities = await resource.GetAllAsync(page, pageSize);
            return mapper.Map<IList<Shipment>>(entities);
        }

        public async Task<IList<Shipment>> GetShipmentsByStatusAsync(ShipmentStatus status)
        {
            var entities = await resource.GetByStatusAsync(status);
            return mapper.Map<IList<Shipment>>(entities);
        }

        public async Task<Guid> CreateShipmentAsync(Shipment shipment, Guid? createdBy)
        {
            ValidateShipmentForCreation(shipment);

            try
            {
                shipment.Status = ShipmentStatus.Created;
                shipment.CreatedBy = createdBy;
                return await resource.SaveAsync(mapper.Map<Resources.Models.Shipment>(shipment));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to create shipment");
                throw new Exception("Unable to create shipment", ex);
            }
        }

        public async Task<Shipment?> UpdateShipmentAsync(Shipment shipment, Guid? updatedBy)
        {
            var existing = await resource.GetByIdAsync(shipment.ID);
            if (existing == null)
            {
                logger.LogWarning("Attempted to update non-existent shipment {ShipmentId}", shipment.ID);
                return null;
            }

            ValidateShipmentForUpdate(shipment);

            shipment.Status = existing.Status;
            shipment.CreatedAt = existing.CreatedAt;
            shipment.CreatedBy = existing.CreatedBy;
            shipment.IsDeleted = existing.IsDeleted;
            shipment.DeletedBy = existing.DeletedBy;
            shipment.DeletedAt = existing.DeletedAt;
            shipment.UpdatedBy = updatedBy;

            try
            {
                await resource.SaveAsync(mapper.Map<Resources.Models.Shipment>(shipment));

                var updated = await resource.GetByIdAsync(shipment.ID);
                return mapper.Map<Shipment>(updated);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                logger.LogWarning(ex, "Concurrency conflict updating shipment {ShipmentId}", shipment.ID);
                throw new Exception(
                    "The shipment was modified by another user. Please refresh and try again.", ex);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to update shipment {ShipmentId}", shipment.ID);
                throw new Exception("Unable to update shipment", ex);
            }
        }

        public async Task DeleteShipmentAsync(Guid id, Guid deletedBy)
        {
            await resource.DeleteAsync(id, deletedBy);
        }

        public async Task<Shipment?> UpdateShipmentStatusAsync(Guid id, ShipmentStatus newStatus, Guid? updatedBy)
        {
            var shipment = await resource.GetByIdAsync(id);
            if (shipment == null)
            {
                return null;
            }

            ValidateStatusTransition(shipment.Status, newStatus);

            shipment.Status = newStatus;
            shipment.UpdatedBy = updatedBy;

            await resource.SaveAsync(shipment);
            return mapper.Map<Shipment>(shipment);
        }

        private void ValidateStatusTransition(ShipmentStatus currentStatus, ShipmentStatus newStatus)
        {
            if (currentStatus == ShipmentStatus.Delivered || currentStatus == ShipmentStatus.Cancelled)
            {
                if (newStatus != currentStatus)
                {
                    throw new InvalidOperationException(
                        $"Cannot change status from {currentStatus} to {newStatus}");
                }
            }
        }

        private void ValidateShipmentForUpdate(Shipment shipment)
        {
            if (string.IsNullOrWhiteSpace(shipment.TrackingNumber))
            {
                throw new InvalidShipmentDataException("Tracking number is required");
            }
            if (string.IsNullOrWhiteSpace(shipment.RecipientName))
            {
                throw new InvalidShipmentDataException("Recipient name is required");
            }
        }

        private void ValidateShipmentForCreation(Shipment shipment)
        {
            if (string.IsNullOrWhiteSpace(shipment.TrackingNumber))
            {
                throw new InvalidShipmentDataException("Tracking number is required");
            }
            if (string.IsNullOrWhiteSpace(shipment.RecipientName))
            {
                throw new InvalidShipmentDataException("Recipient name number is required");
            }
            if (shipment.Status != ShipmentStatus.Created)
            {
                throw new InvalidShipmentDataException("New shipments must have status 'Created'");
            }
        }
    }
}