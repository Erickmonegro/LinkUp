using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Linq;

namespace LinkUpPro.Core.Application.Helpers.Validations
{
    public static class ImageValidator
    {
        private const long MaxFileSize = 5 * 1024 * 1024; // 5 MB
        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };
        private static readonly string[] AllowedContentTypes = { "image/jpeg", "image/pjpeg", "image/png", "image/webp", "image/x-png" };

        public static bool ValidateFile(IFormFile? file, out string errorMessage)
        {
            errorMessage = string.Empty;
            if (file == null || file.Length == 0)
            {
                return true; 
            }

            if (file.Length > MaxFileSize)
            {
                errorMessage = "La imagen seleccionada no puede superar los 5 MB.";
                return false;
            }

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(extension))
            {
                errorMessage = "El archivo seleccionado no tiene un formato de imagen válido.";
                return false;
            }

            if (!AllowedContentTypes.Contains(file.ContentType.ToLowerInvariant()))
            {
                errorMessage = "El archivo seleccionado no tiene un formato de imagen válido.";
                return false;
            }

            if (!ValidateMagicBytes(file))
            {
                errorMessage = "El archivo seleccionado no tiene un formato de imagen válido.";
                return false;
            }

            return true;
        }

        private static bool ValidateMagicBytes(IFormFile file)
        {
            try
            {
                using var stream = file.OpenReadStream();
                using var reader = new BinaryReader(stream);

                if (stream.Length < 12)
                {
                    return false;
                }

                byte[] header = reader.ReadBytes(12);

                // JPEG: FF D8 FF
                if (header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF)
                {
                    return true;
                }

                // PNG: 89 50 4E 47 0D 0A 1A 0A
                if (header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47 &&
                    header[4] == 0x0D && header[5] == 0x0A && header[6] == 0x1A && header[7] == 0x0A)
                {
                    return true;
                }

                // GIF: 47 49 46 38 (GIF87a or GIF89a)
                if (header[0] == 0x47 && header[1] == 0x49 && header[2] == 0x46 && header[3] == 0x38)
                {
                    return true;
                }

                // WebP: RIFF .... WEBP
                if (header[0] == 0x52 && header[1] == 0x49 && header[2] == 0x46 && header[3] == 0x46 &&
                    header[8] == 0x57 && header[9] == 0x45 && header[10] == 0x42 && header[11] == 0x50)
                {
                    return true;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }
    }
}
