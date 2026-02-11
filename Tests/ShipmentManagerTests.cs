using AutoMapper;
using Infrastructure;
using Manager;
using Manager.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Tests
{
    public class ShipmentManagerTests
    {
        private readonly Mock<Resources.IShipmentResource> _mockResource;
        private readonly Mock<ILogger<ShipmentManager>> _mockLogger;
        private readonly Mock<IMapper> _mockMapper;
        private readonly ShipmentManager _manager;

        public ShipmentManagerTests()
        {
            _mockResource = new Mock<Resources.IShipmentResource>();
            _mockLogger = new Mock<ILogger<ShipmentManager>>();
            _mockMapper = new Mock<IMapper>();
            _manager = new ShipmentManager(_mockResource.Object, _mockLogger.Object, _mockMapper.Object);
        }

        [Fact]
        public async Task GetShipmentByIdAsync_WhenShipmentExists_ReturnsShipment()
        {
            var shipmentId = Guid.NewGuid();
            var resourceShipment = new Resources.Models.Shipment
            {
                ID = shipmentId,
                TrackingNumber = "TRACK123",
                RecipientName = "John Doe",
                Status = ShipmentStatus.Created
            };
            var managerShipment = new Shipment
            {
                ID = shipmentId,
                TrackingNumber = "TRACK123",
                RecipientName = "John Doe",
                Status = ShipmentStatus.Created
            };

            _mockResource.Setup(r => r.GetByIdAsync(shipmentId))
                .ReturnsAsync(resourceShipment);
            _mockMapper.Setup(m => m.Map<Shipment>(resourceShipment))
                .Returns(managerShipment);

            var result = await _manager.GetShipmentByIdAsync(shipmentId);

            Assert.NotNull(result);
            Assert.Equal(shipmentId, result.ID);
            Assert.Equal("TRACK123", result.TrackingNumber);
            _mockResource.Verify(r => r.GetByIdAsync(shipmentId), Times.Once);
        }

        [Fact]
        public async Task GetShipmentByIdAsync_WhenShipmentDoesNotExist_ReturnsNull()
        {
            var shipmentId = Guid.NewGuid();
            _mockResource.Setup(r => r.GetByIdAsync(shipmentId))
                .ReturnsAsync((Resources.Models.Shipment?)null);
            _mockMapper.Setup(m => m.Map<Shipment>(null))
                .Returns((Shipment?)null);

            var result = await _manager.GetShipmentByIdAsync(shipmentId);

            Assert.Null(result);
        }

        [Fact]
        public async Task CreateShipmentAsync_WithValidData_CreatesShipment()
        {
            var shipmentId = Guid.NewGuid();
            var createdBy = Guid.NewGuid();
            var shipment = new Shipment
            {
                TrackingNumber = "TRACK123",
                RecipientName = "John Doe",
                Status = ShipmentStatus.Created
            };
            var resourceShipment = new Resources.Models.Shipment
            {
                TrackingNumber = "TRACK123",
                RecipientName = "John Doe",
                Status = ShipmentStatus.Created
            };

            _mockMapper.Setup(m => m.Map<Resources.Models.Shipment>(It.IsAny<Shipment>()))
                .Returns(resourceShipment);
            _mockResource.Setup(r => r.SaveAsync(It.IsAny<Resources.Models.Shipment>()))
                .ReturnsAsync(shipmentId);

            var result = await _manager.CreateShipmentAsync(shipment, createdBy);

            Assert.Equal(shipmentId, result);
            Assert.Equal(ShipmentStatus.Created, shipment.Status);
            Assert.Equal(createdBy, shipment.CreatedBy);
            _mockResource.Verify(r => r.SaveAsync(It.IsAny<Resources.Models.Shipment>()), Times.Once);
        }

        [Fact]
        public async Task CreateShipmentAsync_WithoutTrackingNumber_ThrowsException()
        {
            var shipment = new Shipment
            {
                TrackingNumber = "",
                RecipientName = "John Doe",
                Status = ShipmentStatus.Created
            };

            await Assert.ThrowsAsync<InvalidShipmentDataException>(
                () => _manager.CreateShipmentAsync(shipment, null));
        }

        [Fact]
        public async Task CreateShipmentAsync_WithoutRecipientName_ThrowsException()
        {
            var shipment = new Shipment
            {
                TrackingNumber = "TRACK123",
                RecipientName = "",
                Status = ShipmentStatus.Created
            };

            await Assert.ThrowsAsync<InvalidShipmentDataException>(
                () => _manager.CreateShipmentAsync(shipment, null));
        }

        [Fact]
        public async Task CreateShipmentAsync_WithWrongStatus_ThrowsException()
        {
            var shipment = new Shipment
            {
                TrackingNumber = "TRACK123",
                RecipientName = "John Doe",
                Status = ShipmentStatus.InTransit
            };

            await Assert.ThrowsAsync<InvalidShipmentDataException>(
                () => _manager.CreateShipmentAsync(shipment, null));
        }

        [Fact]
        public async Task UpdateShipmentAsync_WhenShipmentDoesNotExist_ReturnsNull()
        {
            var shipmentId = Guid.NewGuid();
            var shipment = new Shipment
            {
                ID = shipmentId,
                TrackingNumber = "TRACK123",
                RecipientName = "John Doe"
            };

            _mockResource.Setup(r => r.GetByIdAsync(shipmentId))
                .ReturnsAsync((Resources.Models.Shipment?)null);

            var result = await _manager.UpdateShipmentAsync(shipment, null);

            Assert.Null(result);
        }

        [Fact]
        public async Task UpdateShipmentStatusAsync_WithValidTransition_UpdatesStatus()
        {
            var shipmentId = Guid.NewGuid();
            var updatedBy = Guid.NewGuid();
            var existingShipment = new Resources.Models.Shipment
            {
                ID = shipmentId,
                TrackingNumber = "TRACK123",
                RecipientName = "John Doe",
                Status = ShipmentStatus.Created,
                Version = Guid.NewGuid()
            };

            _mockResource.Setup(r => r.GetByIdAsync(shipmentId))
                .ReturnsAsync(existingShipment);
            _mockResource.Setup(r => r.SaveAsync(It.IsAny<Resources.Models.Shipment>()))
                .ReturnsAsync(shipmentId);
            _mockMapper.Setup(m => m.Map<Shipment>(It.IsAny<Resources.Models.Shipment>()))
                .Returns(new Shipment
                {
                    ID = shipmentId,
                    Status = ShipmentStatus.InTransit
                });

            var result = await _manager.UpdateShipmentStatusAsync(
                shipmentId,
                ShipmentStatus.InTransit,
                updatedBy);

            Assert.NotNull(result);
            Assert.Equal(ShipmentStatus.InTransit, result.Status);
            _mockResource.Verify(r => r.SaveAsync(It.Is<Resources.Models.Shipment>(
                s => s.Status == ShipmentStatus.InTransit && s.UpdatedBy == updatedBy)), Times.Once);
        }

        [Fact]
        public async Task UpdateShipmentStatusAsync_WhenShipmentNotFound_ThrowsException()
        {
            var shipmentId = Guid.NewGuid();
            _mockResource.Setup(r => r.GetByIdAsync(shipmentId))
                .ReturnsAsync((Resources.Models.Shipment?)null);

            await Assert.ThrowsAsync<ShipmentNotFoundException>(
                () => _manager.UpdateShipmentStatusAsync(shipmentId, ShipmentStatus.InTransit, null));
        }

        [Fact]
        public async Task UpdateShipmentStatusAsync_FromDeliveredToInTransit_ThrowsException()
        {
            var shipmentId = Guid.NewGuid();
            var existingShipment = new Resources.Models.Shipment
            {
                ID = shipmentId,
                TrackingNumber = "TRACK123",
                RecipientName = "John Doe",
                Status = ShipmentStatus.Delivered,
                Version = Guid.NewGuid()
            };

            _mockResource.Setup(r => r.GetByIdAsync(shipmentId))
                .ReturnsAsync(existingShipment);

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _manager.UpdateShipmentStatusAsync(
                    shipmentId,
                    ShipmentStatus.InTransit,
                    null));
        }

        [Fact]
        public async Task UpdateShipmentStatusAsync_FromCancelledToInTransit_ThrowsException()
        {
            var shipmentId = Guid.NewGuid();
            var existingShipment = new Resources.Models.Shipment
            {
                ID = shipmentId,
                TrackingNumber = "TRACK123",
                RecipientName = "John Doe",
                Status = ShipmentStatus.Cancelled,
                Version = Guid.NewGuid()
            };

            _mockResource.Setup(r => r.GetByIdAsync(shipmentId))
                .ReturnsAsync(existingShipment);

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _manager.UpdateShipmentStatusAsync(
                    shipmentId,
                    ShipmentStatus.InTransit,
                    null));
        }

        [Fact]
        public async Task GetShipmentsByStatusAsync_ReturnsFilteredShipments()
        {
            var resourceShipments = new List<Resources.Models.Shipment>
            {
                new() { ID = Guid.NewGuid(), TrackingNumber = "T1", RecipientName = "John", Status = ShipmentStatus.InTransit },
                new() { ID = Guid.NewGuid(), TrackingNumber = "T2", RecipientName = "Jane", Status = ShipmentStatus.InTransit }
            };
            var managerShipments = new List<Shipment>
            {
                new() { ID = resourceShipments[0].ID, TrackingNumber = "T1", RecipientName = "John", Status = ShipmentStatus.InTransit },
                new() { ID = resourceShipments[1].ID, TrackingNumber = "T2", RecipientName = "Jane", Status = ShipmentStatus.InTransit }
            };

            _mockResource.Setup(r => r.GetByStatusAsync(ShipmentStatus.InTransit))
                .ReturnsAsync(resourceShipments);
            _mockMapper.Setup(m => m.Map<IList<Shipment>>(resourceShipments))
                .Returns(managerShipments);

            var result = await _manager.GetShipmentsByStatusAsync(ShipmentStatus.InTransit);

            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.All(result, s => Assert.Equal(ShipmentStatus.InTransit, s.Status));
        }

        [Fact]
        public async Task GetAllShipmentsAsync_WithPagination_ReturnsPagedResults()
        {
            var resourceShipments = new List<Resources.Models.Shipment>
            {
                new() { ID = Guid.NewGuid(), TrackingNumber = "T1", RecipientName = "John", Status = ShipmentStatus.Created }
            };
            var managerShipments = new List<Shipment>
            {
                new() { ID = resourceShipments[0].ID, TrackingNumber = "T1", RecipientName = "John", Status = ShipmentStatus.Created }
            };

            _mockResource.Setup(r => r.GetAllAsync(2, 25))
                .ReturnsAsync(resourceShipments);
            _mockMapper.Setup(m => m.Map<IList<Shipment>>(resourceShipments))
                .Returns(managerShipments);

            var result = await _manager.GetAllShipmentsAsync(2, 25);

            Assert.NotNull(result);
            Assert.Single(result);
            _mockResource.Verify(r => r.GetAllAsync(2, 25), Times.Once);
        }

        [Fact]
        public async Task DeleteShipmentAsync_CallsResourceDelete()
        {
            var shipmentId = Guid.NewGuid();
            var deletedBy = Guid.NewGuid();

            _mockResource.Setup(r => r.DeleteAsync(shipmentId, deletedBy))
                .Returns(Task.CompletedTask);

            await _manager.DeleteShipmentAsync(shipmentId, deletedBy);

            _mockResource.Verify(r => r.DeleteAsync(shipmentId, deletedBy), Times.Once);
        }
    }
}