using Microsoft.AspNetCore.Mvc;
using iPerfect.Models;
using iPerfect.Services;

namespace iPerfect.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FileFormatController : ControllerBase
    {
        private readonly FileFormatService _fileFormatService;

        public FileFormatController(FileFormatService fileFormatService)
        {
            _fileFormatService = fileFormatService;
        }

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

        [HttpPost("verify")]
        public IActionResult Verify([FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file provided.");

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
    }
}
