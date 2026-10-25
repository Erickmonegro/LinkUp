using AutoMapper;
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
        private readonly IMapper _mapper;
        private readonly IImageStorageService _imageStorageService;

        public PostController(IPostService postService, IMapper mapper, IImageStorageService imageStorageService)
        {
            _postService = postService;
            _mapper = mapper;
            _imageStorageService = imageStorageService;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SavePostViewModel vm)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return RedirectToAction("Login", "Login");

            vm.AuthorId = userId;

            if (string.IsNullOrWhiteSpace(vm.TextContent))
            {
                ModelState.AddModelError("TextContent", "Debe ingresar el contenido de la publicación.");
            }

            if (vm.ContentType == ContentType.Image && vm.ImageFile == null)
            {
                ModelState.AddModelError("ImageFile", "Debe seleccionar una imagen para crear la publicación.");
            }

            if (!ModelState.IsValid)
            {
                TempData["CreatePostError"] = string.Join("<br/>", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                return RedirectToAction("Index", "Home");
            }

            if (vm.ContentType == ContentType.Image && vm.ImageFile != null)
            {
                vm.MediaUrl = await _imageStorageService.UploadImageAsync(vm.ImageFile, userId, "Posts");
            }

            var postDto = _mapper.Map<PostDTO>(vm);
            postDto.Id = 0;
            postDto.TextContent = vm.TextContent.Trim();
            postDto.EditionStatus = EditionStatus.NotEdited;
            postDto.AuthorId = userId;
            postDto.AuthorName = "";

            await _postService.AddAsync(postDto);
            TempData["SuccessMessage"] = "La publicación fue creada correctamente.";
            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> Edit(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return RedirectToAction("Login", "Login");

            var post = await _postService.GetByIdAsync(id);
            if (post == null || !post.IsActive)
            {
                TempData["ErrorMessage"] = "Esta publicación ya no se encuentra disponible.";
                return RedirectToAction("Index", "Home");
            }

            if (post.AuthorId != userId)
            {
                TempData["ErrorMessage"] = "No posee permisos para editar esta publicación.";
                return RedirectToAction("AccessDenied", "Login");
            }

            var vm = _mapper.Map<SavePostViewModel>(post);
            if (post.ContentType != ContentType.YouTubeVideo)
            {
                vm.MediaUrl = null;
            }

            ViewBag.CurrentMediaUrl = post.ContentType == ContentType.Image ? post.MediaUrl : null;
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(SavePostViewModel vm)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return RedirectToAction("Login", "Login");

            var existingPost = await _postService.GetByIdAsync(vm.Id);
            if (existingPost == null || !existingPost.IsActive)
            {
                TempData["ErrorMessage"] = "Esta publicación ya no se encuentra disponible.";
                return RedirectToAction("Index", "Home");
            }

            if (existingPost.AuthorId != userId)
            {
                TempData["ErrorMessage"] = "No posee permisos para editar esta publicación.";
                return RedirectToAction("AccessDenied", "Login");
            }

            if (string.IsNullOrWhiteSpace(vm.TextContent))
            {
                ModelState.AddModelError("TextContent", "Debe ingresar el contenido de la publicación.");
            }

            if (vm.ContentType == ContentType.Image && vm.ImageFile == null && existingPost.ContentType == ContentType.Image)
            {
                vm.MediaUrl = existingPost.MediaUrl;
            }

            if (vm.ContentType == ContentType.Image && vm.ImageFile == null && existingPost.ContentType != ContentType.Image)
            {
                ModelState.AddModelError("ImageFile", "Debe seleccionar una imagen para crear la publicación.");
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
                    finalMediaUrl = await _imageStorageService.UploadImageAsync(vm.ImageFile, userId, "Posts", true, existingPost.MediaUrl);
                }
            }
            else if (vm.ContentType == ContentType.YouTubeVideo)
            {
                finalMediaUrl = vm.MediaUrl;
            }
            else
            {
                finalMediaUrl = null;
            }

            var postDto = _mapper.Map<PostDTO>(vm);
            postDto.Created = existingPost.Created;
            postDto.Updated = DateTime.UtcNow;
            postDto.TextContent = vm.TextContent.Trim();
            postDto.MediaUrl = finalMediaUrl;
            postDto.EditionStatus = EditionStatus.Edited;
            postDto.AuthorId = userId;
            postDto.AuthorName = existingPost.AuthorName;
            postDto.IsActive = true;

            await _postService.UpdateAsync(postDto, vm.Id);
            TempData["SuccessMessage"] = "La publicación fue actualizada correctamente.";
            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> Delete(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return RedirectToAction("Login", "Login");

            var post = await _postService.GetByIdAsync(id);
            if (post == null || !post.IsActive)
            {
                TempData["ErrorMessage"] = "Esta publicación ya no se encuentra disponible.";
                return RedirectToAction("Index", "Home");
            }

            if (post.AuthorId != userId)
            {
                TempData["ErrorMessage"] = "No posee permisos para eliminar esta publicación.";
                return RedirectToAction("AccessDenied", "Login");
            }

            return View(post);
        }

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return RedirectToAction("Login", "Login");

            var post = await _postService.GetByIdAsync(id);
            if (post == null || !post.IsActive)
            {
                TempData["ErrorMessage"] = "Esta publicación ya no se encuentra disponible.";
                return RedirectToAction("Index", "Home");
            }

            if (post.AuthorId != userId)
            {
                TempData["ErrorMessage"] = "No posee permisos para eliminar esta publicación.";
                return RedirectToAction("AccessDenied", "Login");
            }

            post.IsActive = false; 

            await _postService.UpdateAsync(post, post.Id);

            TempData["SuccessMessage"] = "La publicación fue eliminada correctamente.";
            return RedirectToAction("Index", "Home");
        }
    }
}
