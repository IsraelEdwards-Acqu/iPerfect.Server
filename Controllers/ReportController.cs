using Microsoft.AspNetCore.Mvc;
using iPerfect.Models;
using iPerfect.Services;

namespace iPerfect.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReportController : ControllerBase
    {
        private readonly FileFormatService _fileFormatService;

        public ReportController(FileFormatService fileFormatService)
        {
            _fileFormatService = fileFormatService;
        }

        /// <summary>
        /// Upload a .ipeg file and verify its integrity.
        /// </summary>
        [HttpPost("verify")]
        [ProducesResponseType(typeof(ImageReport), 200)]
        [ProducesResponseType(400)]
        public IActionResult Verify([FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No report file provided.");

            try
            {
                using var stream = file.OpenReadStream();
                var report = _fileFormatService.VerifyReport(stream);
                return Ok(report);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Verification failed", details = ex.Message });
            }
        }

        /// <summary>
        /// Export a report object into a signed .ipeg file.
        /// </summary>
        [HttpPost("export")]
        public IActionResult Export([FromBody] ImageReport report)
        {
            try
            {
                var bytes = _fileFormatService.ExportReport(report);
                return File(bytes, "application/octet-stream", "report.ipeg");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Export failed", details = ex.Message });
            }
        }

        /// <summary>
        /// Simple endpoint to confirm the Report API is working.
        /// </summary>
        [HttpGet("ping")]
        public IActionResult Ping()
        {
            return Ok(new { status = "Report API active", timestamp = DateTime.UtcNow });
        }
    }
}
