using System;
using System.Collections.Generic;
using System.IO;
using iPerfect.Models;

namespace iPerfect.Services
{
    public class MetadataAnalyzer
    {
        public MetadataInfo ExtractMetadata(Stream imageStream)
        {
            // Placeholder: in production use SixLabors.ImageSharp or MetadataExtractor
            return new MetadataInfo
            {
                Format = "JPEG",
                MimeType = "image/jpeg",
                CameraMake = "Unknown",
                CameraModel = "Unknown",
                Software = "Unknown",
                CreationTime = DateTime.UtcNow,
                IsEdited = false,
                RawEntries = new List<string> { "Simulated metadata entry" }
            };
        }
    }
}
