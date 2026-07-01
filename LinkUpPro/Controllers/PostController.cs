using LinkUpPro.Core.Application.DTOs.PostDTO;
using LinkUpPro.Core.Application.Interfaces;
using LinkUpPro.Core.Application.ViewModels.Post;
using LinkUpPro.Core.Domain.Common.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace LinkUpPro.Controllers
{
    [Authorize]
    public class PostController : Controller
    {
        private readonly IPostService _postService;

        public PostController(IPostService postService)
        {
            _postService = postService;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SavePostViewModel vm)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return RedirectToAction("Login", "User");

            vm.AuthorId = userId;

            // Custom validation per PDF
            if (string.IsNullOrWhiteSpace(vm.TextContent))
            {
                ModelState.AddModelError("TextContent", "Debe ingresar el contenido de la publicación.");
            }

            if (vm.ContentType == ContentType.Image)
            {
                if (vm.ImageFile == null || vm.ImageFile.Length == 0)
                {
                    ModelState.AddModelError("ImageFile", "Debe seleccionar una imagen para crear la publicación.");
                }
                else
                {
                    var validationError = ValidateImageFile(vm.ImageFile);
                    if (validationError != null)
                    {
                        ModelState.AddModelError("ImageFile", validationError);
                    }
                }
            }
            else if (vm.ContentType == ContentType.YouTubeVideo)
            {
                var youtubeId = GetYouTubeVideoId(vm.MediaUrl ?? "");
                if (string.IsNullOrWhiteSpace(vm.MediaUrl) || youtubeId == null)
                {
                    ModelState.AddModelError("MediaUrl", "Debe ingresar un enlace válido de YouTube.");
                }
                else
                {
                    vm.MediaUrl = $"https://www.youtube.com/embed/{youtubeId}";
                }
            }

            if (!ModelState.IsValid)
            {
                // To display errors, we can redirect back or return index view.
                // The standard way in MVC is to reload the Home/Index with the validation errors
                // or redirect and pass them via TempData/ViewBag.
                // But returning RedirectToAction("Index", "Home") with TempData is standard for PRG pattern.
                TempData["CreatePostError"] = string.Join("<br/>", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                return RedirectToAction("Index", "Home");
            }

            if (vm.ContentType == ContentType.Image && vm.ImageFile != null)
            {
                vm.MediaUrl = await SaveImageFileAsync(vm.ImageFile);
            }

            var postDto = new PostDTO
            {
                Id = 0,
                TextContent = vm.TextContent.Trim(),
                ContentType = vm.ContentType,
                MediaUrl = vm.MediaUrl,
                PrivacyMode = vm.PrivacyMode,
                CommentStatus = vm.CommentStatus,
                EditionStatus = EditionStatus.NotEdited,
                AuthorId = userId,
                AuthorName = ""
            };

            await _postService.AddAsync(postDto);
            TempData["SuccessMessage"] = "La publicación fue creada correctamente.";
            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> Edit(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return RedirectToAction("Login", "User");

            var post = await _postService.GetByIdAsync(id);
            if (post == null || post.AuthorId != userId)
            {
                return RedirectToAction("AccessDenied", "User");
            }

            var vm = new SavePostViewModel
            {
                Id = post.Id,
                TextContent = post.TextContent,
                MediaUrl = post.ContentType == ContentType.YouTubeVideo ? post.MediaUrl : null,
                ContentType = post.ContentType,
                PrivacyMode = post.PrivacyMode,
                CommentStatus = post.CommentStatus,
                EditionStatus = post.EditionStatus,
                AuthorId = post.AuthorId
            };

            ViewBag.CurrentMediaUrl = post.ContentType == ContentType.Image ? post.MediaUrl : null;
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(SavePostViewModel vm)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return RedirectToAction("Login", "User");

            var existingPost = await _postService.GetByIdAsync(vm.Id);
            if (existingPost == null || existingPost.AuthorId != userId)
            {
                return RedirectToAction("AccessDenied", "User");
            }

            // Custom validation per PDF
            if (string.IsNullOrWhiteSpace(vm.TextContent))
            {
                ModelState.AddModelError("TextContent", "Debe ingresar el contenido de la publicación.");
            }

            if (vm.ContentType == ContentType.Image)
            {
                if (vm.ImageFile != null && vm.ImageFile.Length > 0)
                {
                    var validationError = ValidateImageFile(vm.ImageFile);
                    if (validationError != null)
                    {
                        ModelState.AddModelError("ImageFile", validationError);
                    }
                }
            }
            else if (vm.ContentType == ContentType.YouTubeVideo)
            {
                var youtubeId = GetYouTubeVideoId(vm.MediaUrl ?? "");
                if (string.IsNullOrWhiteSpace(vm.MediaUrl) || youtubeId == null)
                {
                    ModelState.AddModelError("MediaUrl", "Debe ingresar un enlace válido de YouTube.");
                }
                else
                {
                    vm.MediaUrl = $"https://www.youtube.com/embed/{youtubeId}";
                }
            }

            if (!ModelState.IsValid)
            {
                ViewBag.CurrentMediaUrl = existingPost.ContentType == ContentType.Image ? existingPost.MediaUrl : null;
                return View(vm);
            }

            string? finalMediaUrl = existingPost.MediaUrl;

            if (vm.ContentType == ContentType.Image)
            {
                if (vm.ImageFile != null && vm.ImageFile.Length > 0)
                {
                    finalMediaUrl = await SaveImageFileAsync(vm.ImageFile);
                }
                else if (existingPost.ContentType != ContentType.Image)
                {
                    // Switched to Image but didn't upload a new one
                    ModelState.AddModelError("ImageFile", "Debe seleccionar una imagen para crear la publicación.");
                    ViewBag.CurrentMediaUrl = null;
                    return View(vm);
                }
            }
            else if (vm.ContentType == ContentType.YouTubeVideo)
            {
                finalMediaUrl = vm.MediaUrl;
            }

            var postDto = new PostDTO
            {
                Id = vm.Id,
                TextContent = vm.TextContent.Trim(),
                ContentType = vm.ContentType,
                MediaUrl = finalMediaUrl,
                PrivacyMode = vm.PrivacyMode,
                CommentStatus = vm.CommentStatus,
                EditionStatus = EditionStatus.Edited,
                AuthorId = userId,
                AuthorName = ""
            };

            await _postService.UpdateAsync(postDto, vm.Id);
            TempData["SuccessMessage"] = "La publicación fue actualizada correctamente.";
            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> Delete(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return RedirectToAction("Login", "User");

            var post = await _postService.GetByIdAsync(id);
            if (post == null || post.AuthorId != userId)
            {
                return RedirectToAction("AccessDenied", "User");
            }

            return View(post);
        }

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return RedirectToAction("Login", "User");

            var post = await _postService.GetByIdAsync(id);
            if (post == null || post.AuthorId != userId)
            {
                return RedirectToAction("AccessDenied", "User");
            }

            // Do a LOGICAL deletion as required by PDF
            post.IsActive = false; // Note: AutoMapper will map this to Post entity which inherits from BasicEntity

            // Wait, we need to map to save it. But IPostService inherits from IGenericService<PostDTO>,
            // and does update. Let's see if we can do update. Yes!
            await _postService.UpdateAsync(post, post.Id);

            TempData["SuccessMessage"] = "La publicación fue eliminada correctamente.";
            return RedirectToAction("Index", "Home");
        }

        #region Private Helpers

        private static string? ValidateImageFile(IFormFile file)
        {
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
            {
                return "El archivo seleccionado no tiene un formato de imagen válido. Solo se permiten .jpg, .jpeg, .png o .webp.";
            }

            if (file.Length > 5 * 1024 * 1024)
            {
                return "La imagen no debe superar los 5 MB.";
            }

            var allowedMimeTypes = new[] { "image/jpeg", "image/png", "image/webp" };
            if (!allowedMimeTypes.Contains(file.ContentType.ToLowerInvariant()))
            {
                return "El archivo seleccionado no tiene un formato de imagen válido.";
            }

            return null;
        }

        private async Task<string> SaveImageFileAsync(IFormFile file)
        {
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "posts");
            Directory.CreateDirectory(uploadsFolder);

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var fileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return $"/uploads/posts/{fileName}";
        }

        private static string? GetYouTubeVideoId(string url)
        {
            if (string.IsNullOrWhiteSpace(url)) return null;

            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            {
                return null;
            }

            var host = uri.Host.ToLower();
            if (!host.Contains("youtube.com") && !host.Contains("youtu.be"))
            {
                return null;
            }

            if (host.Contains("youtu.be"))
            {
                return uri.AbsolutePath.TrimStart('/');
            }

            var query = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(uri.Query);
            if (query.TryGetValue("v", out var videoId))
            {
                return videoId.FirstOrDefault();
            }

            var segments = uri.Segments;
            if (segments.Length > 1 && (segments[1] == "embed/" || segments[1] == "v/"))
            {
                return segments[^1];
            }

            return null;
        }

        #endregion
    }
}
