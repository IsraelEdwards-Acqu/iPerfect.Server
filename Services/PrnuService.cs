using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using iPerfect.Models;

namespace iPerfect.Server.Services
{
    /// <summary>
    /// PRNU (Photo Response Non-Uniformity) Fingerprinting Service
    /// Simplified version using byte-level heuristics.
    /// Stores and matches image fingerprints for authenticity checks.
    /// </summary>
    public class PrnuService
    {
        private readonly string _storePath;

        public PrnuService()
        {
            _storePath = Path.Combine(AppContext.BaseDirectory, "prnu_store");
            Directory.CreateDirectory(_storePath);
        }

        /// <summary>
        /// Register a fingerprint for a camera device.
        /// </summary>
        public async Task RegisterFingerprintAsync(string cameraId, byte[] imageBytes)
        {
            var fingerprint = ExtractFingerprint(imageBytes);
            var path = Path.Combine(_storePath, $"{Sanitize(cameraId)}.bin");
            await File.WriteAllBytesAsync(path, fingerprint);
        }

        /// <summary>
        /// Match an image against all registered fingerprints.
        /// Returns the best match result.
        /// </summary>
        public async Task<PRNUResult> MatchFingerprintAsync(byte[] imageBytes)
        {
            var residual = ExtractFingerprint(imageBytes);
            var best = new PRNUResult
            {
                MatchesReference = false,
                SimilarityScore = 0.0,
                BestCameraId = string.Empty
            };

            var files = Directory.EnumerateFiles(_storePath, "*.bin");
            foreach (var f in files)
            {
                try
                {
                    var stored = await File.ReadAllBytesAsync(f);
                    double score = ComputeNormalizedCorrelation(residual, stored);
                    var id = Path.GetFileNameWithoutExtension(f);

                    if (score > best.SimilarityScore)
                    {
                        best.SimilarityScore = score;
                        best.BestCameraId = id;
                        best.MatchesReference = score > 0.7; // threshold
                    }
                }
                catch
                {
                    // Skip corrupted fingerprints
                }
            }

            return best;
        }

        /// <summary>
        /// Extract PRNU fingerprint from image bytes.
        /// Uses entropy + distribution + repetition heuristics.
        /// </summary>
        private byte[] ExtractFingerprint(byte[] imageBytes)
        {
            if (imageBytes == null || imageBytes.Length == 0)
                throw new ArgumentException("Image bytes cannot be empty");

            var entropy = CalculateEntropy(imageBytes);
            var distribution = AnalyzeByteDistribution(imageBytes);
            var repetition = AnalyzeRepetition(imageBytes);

            var fingerprint = new byte[24];
            Array.Copy(BitConverter.GetBytes(entropy), 0, fingerprint, 0, 8);
            Array.Copy(BitConverter.GetBytes(distribution), 0, fingerprint, 8, 8);
            Array.Copy(BitConverter.GetBytes(repetition), 0, fingerprint, 16, 8);

            return fingerprint;
        }

        private double CalculateEntropy(byte[] data)
        {
            var frequencies = new int[256];
            foreach (var b in data) frequencies[b]++;

            double entropy = 0.0;
            double len = data.Length;
            for (int i = 0; i < 256; i++)
            {
                if (frequencies[i] == 0) continue;
                double p = frequencies[i] / len;
                entropy -= p * Math.Log2(p);
            }
            return entropy;
        }

        private double AnalyzeByteDistribution(byte[] data)
        {
            int sampleStep = Math.Max(1, data.Length / 1000);
            var samples = new List<byte>();
            for (int i = 0; i < data.Length; i += sampleStep)
                samples.Add(data[i]);

            if (samples.Count == 0) return 0.0;

            // Cast bytes to double before averaging
            double mean = samples.Average(b => (double)b);
            double variance = samples.Average(s => Math.Pow(s - mean, 2));

            return Math.Sqrt(variance);
        }

        private double AnalyzeRepetition(byte[] data)
        {
            int matches = 0;
            int total = Math.Min(data.Length - 1, 10000);
            for (int i = 0; i < total; i++)
                if (data[i] == data[i + 1]) matches++;
            return (double)matches / total;
        }

        private double ComputeNormalizedCorrelation(byte[] aBytes, byte[] bBytes)
        {
            int len = Math.Min(aBytes.Length, bBytes.Length) / 8;
            var a = new double[len];
            var b = new double[len];

            Buffer.BlockCopy(aBytes, 0, a, 0, len * 8);
            Buffer.BlockCopy(bBytes, 0, b, 0, len * 8);

            double dot = 0, normA = 0, normB = 0;
            for (int i = 0; i < len; i++)
            {
                dot += a[i] * b[i];
                normA += a[i] * a[i];
                normB += b[i] * b[i];
            }

            return (normA > 0 && normB > 0) ? dot / (Math.Sqrt(normA) * Math.Sqrt(normB)) : 0.0;
        }

        private string Sanitize(string id) =>
            string.Concat(id.Where(c => char.IsLetterOrDigit(c) || c == '_' || c == '-')).ToLowerInvariant();
    }
}
