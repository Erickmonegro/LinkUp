using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace LinkUpPro.Core.Application.Interfaces
{
    public interface IImageStorageService
    {
        Task<string?> UploadImageAsync(IFormFile? file, string id, string folderName, bool isEditMode = false, string? existingImagePath = "");
        bool DeleteImage(string id, string folderName);
    }
}
