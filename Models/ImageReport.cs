using System;
using System.Collections.Generic;

namespace iPerfect.Models
{
    /// <summary>
    /// Unified report for image authenticity analysis.
    /// Includes metadata, forensic checks, AI detection, and PRNU results.
    /// </summary>
    public class ImageReport
    {
        // Basic file info
        public string Format { get; set; } = string.Empty;
        public string MimeType { get; set; } = string.Empty;

        // Metadata
        public string? CameraMake { get; set; }
        public string? CameraModel { get; set; }
        public string? Software { get; set; }
        public DateTime? CreationTime { get; set; }
        public List<string> ExifEntries { get; set; } = new();

        // Forensic analysis
        public bool IsTampered { get; set; }
        public bool IsEdited { get; set; }
        public string ElaPreviewBase64 { get; set; } = string.Empty;
        public double ElaScore { get; set; }

        // AI detection
        public bool IsAIGenerated { get; set; }
        public double AILikelihood { get; set; }

        // PRNU fingerprinting
        public string? PRNUCameraId { get; set; }
        public double PRNUScore { get; set; }
        public bool PRNUMatchesReference { get; set; }

        // Confidence scoring
        public double ConfidenceScore { get; set; }

        // Notes / anomalies
        public List<string> Alterations { get; set; } = new();
    }
}
