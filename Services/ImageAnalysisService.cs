using System.IO;
using System.Threading.Tasks;
using iPerfect.Models;
using iPerfect.Server.Services;

namespace iPerfect.Services
{
    /// <summary>
    /// Central orchestrator for image analysis.
    /// Runs metadata extraction, ELA, AI detection, and PRNU fingerprinting.
    /// </summary>
    public class ImageAnalysisService
    {
        private readonly MetadataAnalyzer _metadataAnalyzer;
        private readonly ElaAnalyzer _elaAnalyzer;
        private readonly AiService _aiService;
        private readonly PrnuService _prnuService;

        public ImageAnalysisService(
            MetadataAnalyzer metadataAnalyzer,
            ElaAnalyzer elaAnalyzer,
            AiService aiService,
            PrnuService prnuService)
        {
            _metadataAnalyzer = metadataAnalyzer;
            _elaAnalyzer = elaAnalyzer;
            _aiService = aiService;
            _prnuService = prnuService;
        }

        public async Task<ImageReport> AnalyzeImageAsync(Stream imageStream)
        {
            var report = new ImageReport();

            // --- METADATA ---
            var metadata = _metadataAnalyzer.ExtractMetadata(imageStream);
            report.Format = metadata.Format;
            report.MimeType = metadata.MimeType;
            report.CameraMake = metadata.CameraMake;
            report.CameraModel = metadata.CameraModel;
            report.Software = metadata.Software;
            report.CreationTime = metadata.CreationTime;
            report.ExifEntries = metadata.RawEntries;
            report.IsEdited = metadata.IsEdited;

            imageStream.Position = 0;

            // --- ELA ---
            var elaResult = _elaAnalyzer.RunEla(imageStream);
            report.ElaPreviewBase64 = elaResult.PreviewBase64;
            report.ElaScore = elaResult.Score;
            report.IsTampered = elaResult.Score > 0.5;

            imageStream.Position = 0;

            // --- AI DETECTION ---
            using var ms = new MemoryStream();
            await imageStream.CopyToAsync(ms);
            var aiResult = await _aiService.AnalyzeAsync(ms.ToArray());
            report.IsAIGenerated = aiResult.Score > 0.6; // threshold
            report.AILikelihood = aiResult.Score;
            report.Alterations.AddRange(aiResult.Details);

            // --- PRNU ---
            var prnuResult = await _prnuService.MatchFingerprintAsync(ms.ToArray());
            report.PRNUCameraId = prnuResult.BestCameraId;
            report.PRNUScore = prnuResult.SimilarityScore;
            report.PRNUMatchesReference = prnuResult.MatchesReference;

            // --- CONFIDENCE SCORING ---
            // Weighted combination of signals
            double elaWeight = 0.4;
            double aiWeight = 0.4;
            double prnuWeight = 0.2;

            report.ConfidenceScore =
                (report.ElaScore * elaWeight) +
                (report.AILikelihood * aiWeight) +
                (report.PRNUScore * prnuWeight);

            return report;
        }
    }
}
