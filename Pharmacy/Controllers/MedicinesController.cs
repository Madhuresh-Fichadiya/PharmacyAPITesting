using Microsoft.AspNetCore.Mvc;
using Pharmacy.API.Services;

namespace Pharmacy.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MedicinesController : ControllerBase
    {
        private readonly IInventoryService _inventoryService;

        // The service is injected via the constructor
        public MedicinesController(IInventoryService inventoryService)
        {
            _inventoryService = inventoryService;
        }

        [HttpPost("{id}/sell")]
        public async Task<IActionResult> SellMedicine(int id, [FromBody] int quantity)
        {
            try
            {
                // The controller calls the service to handle the business rules
                bool success = await _inventoryService.ProcessSaleAsync(id, quantity);

                if (!success)
                {
                    return BadRequest("Insufficient stock to complete the sale.");
                }

                return Ok("Sale processed successfully.");
            }
            catch (KeyNotFoundException)
            {
                return NotFound($"Medicine with ID {id} not found.");
            }
            catch (InvalidOperationException ex)
            {
                // Handles specific business rule violations (like expired medicine)
                return UnprocessableEntity(ex.Message);
            }
        }
    }
}
