using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace iPerfect.Server.Services
{
    public class AiService
    {
        private readonly IHttpClientFactory _httpFactory;
        private readonly string? _remoteEndpoint;

        public AiService(IHttpClientFactory httpFactory, IConfiguration config)
        {
            _httpFactory = httpFactory;
            _remoteEndpoint = config["AIDetector:Endpoint"];
        }

        public async Task<(double Score, List<string> Details)> AnalyzeAsync(byte[] imageBytes)
        {
            if (!string.IsNullOrEmpty(_remoteEndpoint))
            {
                try
                {
                    var client = _httpFactory.CreateClient();
                    var payload = new { image_base64 = Convert.ToBase64String(imageBytes) };
                    var resp = await client.PostAsJsonAsync(_remoteEndpoint, payload);
                    if (resp.IsSuccessStatusCode)
                    {
                        var r = await resp.Content.ReadFromJsonAsync<RemoteResult>();
                        if (r != null) 
                            return (Math.Clamp(r.Score, 0.0, 1.0), r.Details ?? new List<string> { "Remote AI analysis completed" });
                    }
                }
                catch { }
            }

            return AnalyzeLocal(imageBytes);
        }

        private (double Score, List<string> Details) AnalyzeLocal(byte[] bytes)
        {
            var details = new List<string>();
            try
            {
                if (bytes == null || bytes.Length == 0)
                    return (0.0, new List<string> { "Empty image" });

                double entropy = CalculateEntropy(bytes);
                double byteDistribution = AnalyzeByteDistribution(bytes);
                double repetitionRatio = AnalyzeRepetition(bytes);

                double entropySuspicion = Math.Clamp((entropy - 4.0) / 2.0, 0.0, 1.0);
                double distributionSuspicion = Math.Clamp((byteDistribution - 100) / 150.0, 0.0, 1.0);
                double repetitionScore = 1.0 - Math.Clamp((repetitionRatio - 0.1) / 0.3, 0.0, 1.0);

                double score = (entropySuspicion * 0.40) + (distributionSuspicion * 0.35) + (repetitionScore * 0.25);
                score = Math.Clamp(score, 0.0, 1.0);

                details.Add($"Byte entropy: {entropy:F2}");
                details.Add($"Byte distribution variance: {byteDistribution:F1}");
                details.Add($"Repetition ratio: {repetitionRatio:P1}");
                details.Add($"Confidence score: {score:P0}");
                details.Add("Server-side heuristic analysis");

                return (score, details);
            }
            catch (Exception ex)
            {
                details.Add($"Server analysis error: {ex.Message}");
                return (0.0, details);
            }
        }

        private double CalculateEntropy(byte[] data)
        {
            if (data == null || data.Length == 0) return 0.0;

            var frequencies = new int[256];
            foreach (var b in data)
                frequencies[b]++;

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
            if (data == null || data.Length < 100) return 0.0;

            int sampleStep = Math.Max(1, data.Length / 1000);
            var samples = new List<byte>();
            for (int i = 0; i < data.Length; i += sampleStep)
                samples.Add(data[i]);

            if (samples.Count == 0) return 0.0;

            double mean = 0;
            foreach (var s in samples)
                mean += s;
            mean /= samples.Count;

            double variance = 0;
            foreach (var s in samples)
                variance += Math.Pow(s - mean, 2);
            variance /= samples.Count;

            return Math.Sqrt(variance);
        }

        private double AnalyzeRepetition(byte[] data)
        {
            if (data == null || data.Length < 10) return 0.0;

            int matches = 0;
            int total = Math.Min(data.Length - 1, 10000);

            for (int i = 0; i < total; i++)
            {
                if (i + 1 < data.Length && data[i] == data[i + 1])
                    matches++;
            }

            return (double)matches / total;
        }

        private class RemoteResult 
        { 
            public double Score { get; set; } 
            public List<string>? Details { get; set; } 
        }
    }
}