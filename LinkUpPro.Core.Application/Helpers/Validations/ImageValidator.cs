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
                errorMessage = "El archivo excede el tamaño máximo permitido de 5 MB.";
                return false;
            }

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(extension))
            {
                errorMessage = "Formato de archivo no válido. Solo se permiten imágenes .jpg, .jpeg, .png o .webp.";
                return false;
            }

            if (!AllowedContentTypes.Contains(file.ContentType.ToLowerInvariant()))
            {
                errorMessage = "El tipo de contenido (MIME) del archivo no es válido para una imagen.";
                return false;
            }

            if (!ValidateMagicBytes(file))
            {
                errorMessage = "El contenido del archivo está dañado o no corresponde a una imagen válida.";
                return false;
            }

            return true;
        }

        private static bool ValidateMagicBytes(IFormFile file)
        {
            try
            {
                using var stream = file.OpenReadStream();
                if (stream.Length < 12) return false;

                byte[] buffer = new byte[12];
                stream.ReadExactly(buffer, 0, 12);

                // JPEG: FF D8 FF
                if (buffer[0] == 0xFF && buffer[1] == 0xD8 && buffer[2] == 0xFF)
                {
                    return true;
                }

                // PNG: 89 50 4E 47 0D 0A 1A 0A
                if (buffer[0] == 0x89 && buffer[1] == 0x50 && buffer[2] == 0x4E && buffer[3] == 0x47 &&
                    buffer[4] == 0x0D && buffer[5] == 0x0A && buffer[6] == 0x1A && buffer[7] == 0x0A)
                {
                    return true;
                }

                // WEBP: RIFF....WEBP
                if (buffer[0] == 0x52 && buffer[1] == 0x49 && buffer[2] == 0x46 && buffer[3] == 0x46 &&
                    buffer[8] == 0x57 && buffer[9] == 0x45 && buffer[10] == 0x42 && buffer[11] == 0x50)
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
