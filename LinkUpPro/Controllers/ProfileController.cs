using AutoMapper;
using LinkUpPro.Core.Application.DTOs.User;
using LinkUpPro.Core.Application.Interfaces;
using LinkUpPro.Core.Application.ViewModels.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;

namespace LinkUpPro.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly IUserService _userService;
        private readonly IPostService _postService;
        private readonly IFriendshipService _friendshipService;
        private readonly IMapper _mapper;
        private readonly IImageStorageService _imageStorageService;

        public ProfileController(IUserService userService, IPostService postService, IFriendshipService friendshipService, IMapper mapper, IImageStorageService imageStorageService)
        {
            _userService = userService;
            _postService = postService;
            _friendshipService = friendshipService;
            _mapper = mapper;
            _imageStorageService = imageStorageService;
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return RedirectToAction("Login", "Login");

            var user = await _userService.GetUserById(userId);
            if (user == null) return RedirectToAction("Login", "Login");

            var vm = _mapper.Map<UpdateUserViewModel>(user);

            var friendsCount = await _friendshipService.GetActiveFriendsCountAsync(userId);
            var posts = await _postService.GetUserPostsFilteredAsync(userId, null, null, null, null, null);

            ViewBag.FriendsCount = friendsCount;
            ViewBag.Posts = posts ?? new List<Core.Application.DTOs.PostDTO.PostDTO>();

            return View("Index", vm);
        }

        public async Task<IActionResult> Edit()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return RedirectToAction("Login", "Login");

            var user = await _userService.GetUserById(userId);
            if (user == null) return RedirectToAction("Login", "Login");

            var vm = _mapper.Map<UpdateUserViewModel>(user);
            vm.CurrentPassword = null;
            vm.Password = null;
            vm.ConfirmPassword = null;

            return View("Edit", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UpdateUserViewModel vm)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return RedirectToAction("Login", "Login");

            if (vm.Id != userId)
            {
                TempData["ErrorMessage"] = "No posee permisos para modificar este perfil.";
                return RedirectToAction("Index", "Home");
            }

            var currentUser = await _userService.GetUserById(userId);
            if (currentUser == null) return RedirectToAction("Login", "Login");

            // Blindar datos que no deben modificarse
            vm.UserName = currentUser.UserName;
            vm.Email = currentUser.Email;

            if (!ModelState.IsValid)
            {
                vm.PhotoUrl = currentUser.PhotoUrl;
                vm.CoverPhotoUrl = currentUser.CoverPhotoUrl;
                return View("Edit", vm);
            }

            // Subir nueva foto de perfil si se proporcionó
            if (vm.ProfileImageFile != null && vm.ProfileImageFile.Length > 0)
            {
                vm.PhotoUrl = await _imageStorageService.UploadImageAsync(vm.ProfileImageFile, userId, "Users", false);
            }
            else
            {
                vm.PhotoUrl = currentUser.PhotoUrl;
            }

            // Subir portada si aplica
            if (vm.CoverImageFile != null && vm.CoverImageFile.Length > 0)
            {
                vm.CoverPhotoUrl = await _imageStorageService.UploadImageAsync(vm.CoverImageFile, userId, "Covers", false);
            }
            else
            {
                vm.CoverPhotoUrl = currentUser.CoverPhotoUrl;
            }

            // Actualizar información personal
            var origin = $"{Request.Scheme}://{Request.Host}";
            var saveDto = _mapper.Map<SaveUserDto>(vm);
            saveDto.Id = userId;
            saveDto.FirstName = vm.FirstName.Trim();
            saveDto.LastName = vm.LastName.Trim();
            saveDto.PhoneNumber = vm.PhoneNumber?.Trim() ?? string.Empty;
            saveDto.Password = "";
            saveDto.Role = currentUser.Role;
            saveDto.PhotoUrl = vm.PhotoUrl;
            saveDto.CoverPhotoUrl = vm.CoverPhotoUrl;

            var updateResponse = await _userService.EditUser(saveDto, origin, false);

            if (updateResponse.HasError)
            {
                // Limpiar archivos nuevos en caso de error para no dejar huérfanos ni afectar la foto actual
                if (vm.PhotoUrl != currentUser.PhotoUrl)
                {
                    _imageStorageService.DeleteImageFile(vm.PhotoUrl);
                    vm.PhotoUrl = currentUser.PhotoUrl;
                }
                if (vm.CoverPhotoUrl != currentUser.CoverPhotoUrl)
                {
                    _imageStorageService.DeleteImageFile(vm.CoverPhotoUrl);
                    vm.CoverPhotoUrl = currentUser.CoverPhotoUrl;
                }
                ViewBag.HasError = true;
                ViewBag.Errors = updateResponse.Errors;
                return View("Edit", vm);
            }

            // Reemplazo seguro: eliminar las fotos anteriores únicamente si la persistencia fue exitosa
            if (vm.PhotoUrl != currentUser.PhotoUrl && !string.IsNullOrWhiteSpace(currentUser.PhotoUrl))
            {
                _imageStorageService.DeleteImageFile(currentUser.PhotoUrl);
            }
            if (vm.CoverPhotoUrl != currentUser.CoverPhotoUrl && !string.IsNullOrWhiteSpace(currentUser.CoverPhotoUrl))
            {
                _imageStorageService.DeleteImageFile(currentUser.CoverPhotoUrl);
            }

            // Si se solicitó cambio de contraseña
            bool isChangingPassword = !string.IsNullOrWhiteSpace(vm.Password) && !string.IsNullOrWhiteSpace(vm.CurrentPassword);
            if (isChangingPassword)
            {
                var passResponse = await _userService.ChangePasswordAsync(userId, vm.CurrentPassword!, vm.Password!);
                if (passResponse.HasError)
                {
                    ViewBag.HasError = true;
                    ViewBag.Errors = passResponse.Errors;
                    return View("Edit", vm);
                }

                // Cerrar sesión e invalidar sesiones activas
                await _userService.SignOutAsync();
                HttpContext.Session.Clear();
                TempData["SuccessMessage"] = "Su perfil y contraseña fueron actualizados correctamente. Inicie sesión nuevamente.";
                return RedirectToAction("Login", "Login");
            }

            TempData["SuccessMessage"] = "Su perfil fue actualizado correctamente.";
            return RedirectToAction("Index", "Home");
        }
    }
}
