using AutoMapper;
using Client.Models;
using Infrastructure;
using Manager;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Client.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ShipmentsController(
        IShipmentManager manager,
        IMapper mapper
        ) : ControllerBase
    {
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<Shipment>> GetById(Guid id)
        {
            var shipment = await manager.GetShipmentByIdAsync(id);
            if (shipment == null)
                return NotFound(new { message = $"Shipment with ID {id} not found" });
            return Ok(mapper.Map<Shipment>(shipment));
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Shipment>>> GetAll(int page = 1, int pageSize = 50)
        {
            var shipments = await manager.GetAllShipmentsAsync(page, pageSize);
            return Ok(mapper.Map<IEnumerable<Shipment>>(shipments));
        }

        [HttpGet("by-status/{status}")]
        public async Task<ActionResult<IEnumerable<Shipment>>> GetByStatus(ShipmentStatus status)
        {
            var shipments = await manager.GetShipmentsByStatusAsync(status);
            return Ok(mapper.Map<IEnumerable<Shipment>>(shipments));
        }

        [HttpPost]
        public async Task<ActionResult<Shipment>> Create([FromBody] ShipmentPost request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var shipment = mapper.Map<Manager.Models.Shipment>(request);
            var shipmentId = await manager.CreateShipmentAsync(shipment, null);
            var created = await manager.GetShipmentByIdAsync(shipmentId);
            var dto = mapper.Map<Shipment>(created);
            return CreatedAtAction(nameof(GetById), new { id = shipmentId }, dto);
        }

        [HttpPatch("{id:guid}/status")]
        public async Task<ActionResult<Shipment>> UpdateStatus(Guid id, [FromBody] UpdateStatusRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var updated = await manager.UpdateShipmentStatusAsync(id, request.Status, null);
            return Ok(mapper.Map<Shipment>(updated));
        }

        [HttpPut("{id:guid}")]
        public async Task<ActionResult<Shipment>> Update(Guid id, [FromBody] ShipmentPut request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var shipment = mapper.Map<Manager.Models.Shipment>(request);
            shipment.ID = id;

            var updated = await manager.UpdateShipmentAsync(shipment, null);
            if (updated == null)
                return NotFound(new { message = $"Shipment with ID {id} not found" });

            return Ok(mapper.Map<Shipment>(updated));
        }
    }
}
