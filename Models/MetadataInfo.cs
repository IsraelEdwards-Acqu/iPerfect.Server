using System;
using System.Collections.Generic;

namespace iPerfect.Models
{
    public class MetadataInfo
    {
        public string Format { get; set; } = string.Empty;
        public string MimeType { get; set; } = string.Empty;
        public string? CameraMake { get; set; }
        public string? CameraModel { get; set; }
        public string? Software { get; set; }
        public DateTime? CreationTime { get; set; }
        public bool IsEdited { get; set; }
        public List<string> RawEntries { get; set; } = new();
    }
}
