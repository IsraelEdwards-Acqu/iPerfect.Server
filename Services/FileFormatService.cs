using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using iPerfect.Models;

namespace iPerfect.Services
{
    /// <summary>
    /// Handles export and verification of .ipeg forensic reports.
    /// Adds cryptographic signing for tamper-proof integrity.
    /// </summary>
    public class FileFormatService
    {
        private readonly RSA _rsa;

        public FileFormatService()
        {
            // Generate or load RSA key pair
            _rsa = RSA.Create(2048);
        }

        /// <summary>
        /// Export an ImageReport as a signed .ipeg file.
        /// </summary>
        public byte[] ExportReport(ImageReport report)
        {
            var json = JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true });
            var dataBytes = Encoding.UTF8.GetBytes(json);

            // Sign the data
            var signature = _rsa.SignData(dataBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

            // Package: [report JSON][delimiter][signature Base64]
            var package = json + "\n---SIGNATURE---\n" + Convert.ToBase64String(signature);
            return Encoding.UTF8.GetBytes(package);
        }

        /// <summary>
        /// Verify a .ipeg file and return the ImageReport if valid.
        /// </summary>
        public ImageReport VerifyReport(Stream fileStream)
        {
            using var reader = new StreamReader(fileStream);
            var content = reader.ReadToEnd();

            var parts = content.Split("\n---SIGNATURE---\n", StringSplitOptions.None);
            if (parts.Length != 2)
                throw new InvalidDataException("Invalid .ipeg file format.");

            var json = parts[0];
            var signature = Convert.FromBase64String(parts[1]);
            var dataBytes = Encoding.UTF8.GetBytes(json);

            // Verify signature
            bool valid = _rsa.VerifyData(dataBytes, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            if (!valid)
                throw new InvalidDataException("Signature verification failed. Report may have been tampered with.");

            return JsonSerializer.Deserialize<ImageReport>(json) ?? new ImageReport();
        }
    }
}
