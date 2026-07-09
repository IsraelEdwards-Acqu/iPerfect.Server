using System.IO;

namespace iPerfect.Services
{
    public class ElaAnalyzer
    {
        public (string PreviewBase64, double Score) RunEla(Stream imageStream)
        {
            // Placeholder: real ELA would recompress and compare pixel differences
            return ("", 0.3); // Example: no tampering detected
        }
    }
}
