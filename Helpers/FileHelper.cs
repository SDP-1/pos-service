using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace pos_service.Helpers
{
    public static class FileHelper
    {
        /// <summary>
        /// Converts an uploaded IFormFile to a Base64 encoded string.
        /// Returns null if the file is null or empty.
        /// NOTE: kept for backward-compatibility; prefer ConvertFileToBytesAsync when storing binary in DB.
        /// </summary>
        public static async Task<string?> ConvertFileToBase64Async(IFormFile? file)
        {
            if (file == null || file.Length == 0)
                return null;

            using (var memoryStream = new MemoryStream())
            {
                await file.CopyToAsync(memoryStream);
                var fileBytes = memoryStream.ToArray();
                return Convert.ToBase64String(fileBytes);
            }
        }

        /// <summary>
        /// Converts an uploaded IFormFile to a byte[] (raw bytes).
        /// Use this when you store images as binary (BLOB/MEDIUMBLOB/LONGBLOB) in the DB.
        /// Returns null if the file is null or empty.
        /// </summary>
        public static async Task<byte[]?> ConvertFileToBytesAsync(IFormFile? file)
        {
            if (file == null || file.Length == 0)
                return null;

            using (var memoryStream = new MemoryStream())
            {
                await file.CopyToAsync(memoryStream);
                return memoryStream.ToArray();
            }
        }
    }
}
