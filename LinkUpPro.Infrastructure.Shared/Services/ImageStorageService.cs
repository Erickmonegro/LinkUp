using LinkUpPro.Core.Application.Helpers.Validations;
using LinkUpPro.Core.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Threading.Tasks;

namespace LinkUpPro.Infrastructure.Shared.Services
{
    public class ImageStorageService : IImageStorageService
    {
        public async Task<string?> UploadImageAsync(IFormFile? file, string id, string folderName, bool isEditMode = false, string? existingImagePath = "")
        {
            if (isEditMode && file == null)
            {
                return existingImagePath;
            }

            if (file == null || file.Length == 0)
            {
                return string.Empty;
            }

            if (!ImageValidator.ValidateFile(file, out string errorMessage))
            {
                throw new ArgumentException(errorMessage);
            }

            string basePath = $"Images/{folderName}/{id}";
            string path = Path.Combine(Directory.GetCurrentDirectory(), $"wwwroot/{basePath}");

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            Guid guid = Guid.NewGuid();
            FileInfo fileInfo = new(file.FileName);
            string fileName = guid + fileInfo.Extension;

            string fullFilePath = Path.Combine(path, fileName);

            using (var stream = new FileStream(fullFilePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return $"/{basePath}/{fileName}";
        }

        public bool DeleteImage(string id, string folderName)
        {
            string basePath = $"Images/{folderName}/{id}";
            string path = Path.Combine(Directory.GetCurrentDirectory(), $"wwwroot/{basePath}");

            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
                return true;
            }

            return false;
        }

        public bool DeleteImageFile(string? relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath)) return false;

            try
            {
                var cleanPath = relativePath.TrimStart('/', '\\').Replace('\\', '/');
                var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", cleanPath);

                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                    return true;
                }
            }
            catch
            {
                // Silently ignore if file is locked or already deleted
            }

            return false;
        }
    }
}
