using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client.Extensions.Msal;
using Pharmacy.API.Services;

namespace Pharmacy.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MedicinesController : ControllerBase
    {
        private readonly IInventoryService _inventoryService;
        private readonly SupabaseService _supabaseService;

        private const long MaxFileSize = 5 * 1024 * 1024; // 5 MB
        // The service is injected via the constructor
        public MedicinesController(IInventoryService inventoryService, SupabaseService supabaseService)
        {
            _inventoryService = inventoryService;
            _supabaseService = supabaseService;
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

        // 1️⃣ Single File Upload
        [HttpPost("upload")]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return BadRequest("File not provided.");

                var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);

                using var stream = file.OpenReadStream();

                var url = await _supabaseService.UploadStreamAsync(stream, fileName, file.ContentType);

                return Ok(new
                {
                    message = "File uploaded successfully",
                    url
                });
            }
            catch (ApplicationException ex)
            {
                Console.WriteLine($"Upload failed due to service issue: {ex.Message}");
                return StatusCode(503, new { message = ex.Message });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected server error during upload: {ex.Message}");
                return StatusCode(500, new
                {
                    message = "Internal server error."
                });
            }
        }

        // 2️⃣ Multiple File Upload
        [HttpPost("upload-multiple")]
        public async Task<IActionResult> UploadMultiple(List<IFormFile> files)
        {
            try
            {
                if (files == null || files.Count == 0)
                    return BadRequest("No files provided.");

                var urls = new List<string>();

                foreach (var file in files)
                {
                    var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);

                    using var stream = file.OpenReadStream();

                    var url = await _supabaseService.UploadStreamAsync(stream, fileName, file.ContentType);

                    urls.Add(url);
                }

                return Ok(new
                {
                    message = "Files uploaded successfully",
                    files = urls
                });
            }
            catch (ApplicationException ex)
            {
                Console.WriteLine($"Storage upload failure: {ex.Message}");
                return StatusCode(503, new { message = ex.Message });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error while uploading multiple files: {ex.Message}");

                return StatusCode(500, new
                {
                    message = "Internal server error"
                });
            }
        }

        [HttpPost("upload-validated")]
        public async Task<IActionResult> UploadValidated(IFormFile file)
        {
            try
            {
                if (file == null)
                    return BadRequest("File is required.");

                if (file.Length > MaxFileSize)
                    return BadRequest("File size exceeds 5MB.");

                var allowedTypes = new[] { ".jpg", ".png", ".jpeg", ".pdf" };

                var ext = Path.GetExtension(file.FileName).ToLower();

                if (!allowedTypes.Contains(ext))
                    return BadRequest("File type not allowed.");

                var fileName = Guid.NewGuid() + ext;

                using var stream = file.OpenReadStream();

                var url = await _supabaseService.UploadStreamAsync(stream, fileName, file.ContentType);

                return Ok(new
                {
                    message = "File uploaded successfully",
                    url
                });
            }
            catch (ApplicationException ex)
            {
                Console.WriteLine($"Storage error during validated upload: {ex.Message}");

                return StatusCode(503, new
                {
                    message = ex.Message
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error during validated upload: {ex.Message}");

                return StatusCode(500, new
                {
                    message = "Internal server error"
                });
            }
        }
    }
}   