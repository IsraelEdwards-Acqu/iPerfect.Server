using Microsoft.AspNetCore.Mvc;
using iPerfect.Models;
using iPerfect.Services;
using iPerfect.Server.Services;

namespace iPerfect.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ImageAnalysisController : ControllerBase
    {
        private readonly ImageAnalysisService _analysisService;
        private readonly PrnuService _prnuService;

        public ImageAnalysisController(ImageAnalysisService analysisService, PrnuService prnuService)
        {
            _analysisService = analysisService;
            _prnuService = prnuService;
        }

        [HttpPost("analyze")]
        [ProducesResponseType(typeof(ImageReport), 200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> Analyze([FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No image file provided.");

            try
            {
                using var stream = file.OpenReadStream();
                var report = await _analysisService.AnalyzeImageAsync(stream);

                // Run PRNU fingerprint matching
                stream.Position = 0;
                using var ms = new MemoryStream();
                await stream.CopyToAsync(ms);
                var prnuResult = await _prnuService.MatchFingerprintAsync(ms.ToArray());

                report.PRNUCameraId = prnuResult.BestCameraId;
                report.PRNUScore = prnuResult.SimilarityScore;
                report.PRNUMatchesReference = prnuResult.MatchesReference;

                return Ok(report);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Image analysis failed", details = ex.Message });
            }
        }
    }
}
