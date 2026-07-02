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

        public ProfileController(IUserService userService, IPostService postService, IFriendshipService friendshipService)
        {
            _userService = userService;
            _postService = postService;
            _friendshipService = friendshipService;
        }

        // GET: /Profile/Index -> Muro del Perfil (Profile Wall)
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return RedirectToAction("Login", "User");

            var user = await _userService.GetUserById(userId);
            if (user == null) return RedirectToAction("Login", "User");

            var vm = new UpdateUserViewModel
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                UserName = user.UserName,
                PhoneNumber = user.PhoneNumber,
                PhotoUrl = user.PhotoUrl,
                CoverPhotoUrl = user.CoverPhotoUrl
            };

            var friendsCount = await _friendshipService.GetActiveFriendsCountAsync(userId);
            var posts = await _postService.GetUserPostsFilteredAsync(userId, null, null, null, null, null);

            ViewBag.FriendsCount = friendsCount;
            ViewBag.Posts = posts ?? new List<Core.Application.DTOs.PostDTO.PostDTO>();

            return View("Index", vm);
        }

        // GET: /Profile/Edit -> Vista para editar todo el perfil
        public async Task<IActionResult> Edit()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return RedirectToAction("Login", "User");

            var user = await _userService.GetUserById(userId);
            if (user == null) return RedirectToAction("Login", "User");

            var vm = new UpdateUserViewModel
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                UserName = user.UserName,
                PhoneNumber = user.PhoneNumber,
                PhotoUrl = user.PhotoUrl,
                CoverPhotoUrl = user.CoverPhotoUrl
            };

            return View("Edit", vm);
        }

        // POST: /Profile/Edit -> Guardar cambios del perfil
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UpdateUserViewModel vm)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return RedirectToAction("Login", "User");

            if (vm.Id != userId)
            {
                return RedirectToAction("AccessDenied", "User");
            }

            var currentUser = await _userService.GetUserById(userId);
            if (currentUser == null) return RedirectToAction("Login", "User");

            vm.UserName = currentUser.UserName;
            vm.Email = currentUser.Email;

            if (vm.ProfileImageFile != null && vm.ProfileImageFile.Length > 0)
            {
                var validationError = ValidateImageFile(vm.ProfileImageFile);
                if (validationError != null)
                {
                    ModelState.AddModelError("ProfileImageFile", validationError);
                }
                else
                {
                    vm.PhotoUrl = await SaveImageFileAsync(vm.ProfileImageFile);
                }
            }
            else if (!string.IsNullOrWhiteSpace(vm.PhotoUrl))
            {
                vm.PhotoUrl = vm.PhotoUrl.Trim();
            }
            else
            {
                vm.PhotoUrl = currentUser.PhotoUrl;
            }

            if (vm.CoverImageFile != null && vm.CoverImageFile.Length > 0)
            {
                var validationError = ValidateImageFile(vm.CoverImageFile);
                if (validationError != null)
                {
                    ModelState.AddModelError("CoverImageFile", validationError);
                }
                else
                {
                    vm.CoverPhotoUrl = await SaveImageFileAsync(vm.CoverImageFile);
                }
            }
            else if (!string.IsNullOrWhiteSpace(vm.CoverPhotoUrl))
            {
                vm.CoverPhotoUrl = vm.CoverPhotoUrl.Trim();
            }
            else
            {
                vm.CoverPhotoUrl = currentUser.CoverPhotoUrl;
            }

            bool isChangingPassword = !string.IsNullOrEmpty(vm.Password);
            if (isChangingPassword)
            {
                if (string.IsNullOrEmpty(vm.CurrentPassword))
                {
                    ModelState.AddModelError("CurrentPassword", "Debe ingresar su contraseña actual para realizar el cambio.");
                }
                
                if (vm.Password != vm.ConfirmPassword)
                {
                    ModelState.AddModelError("ConfirmPassword", "La nueva contraseña y su confirmación no coinciden.");
                }
            }

            if (!ModelState.IsValid)
            {
                return View("Edit", vm);
            }

            if (isChangingPassword)
            {
                var passResponse = await _userService.ChangePasswordAsync(userId, vm.CurrentPassword!, vm.Password!);
                if (passResponse.HasError)
                {
                    foreach (var error in passResponse.Errors)
                    {
                        if (error.Contains("Passwords must have at least one non alphanumeric character", StringComparison.OrdinalIgnoreCase))
                        {
                            ModelState.AddModelError("CurrentPassword", "La contraseña debe tener al menos un carácter no alfanumérico.");
                        }
                        else if (error.Contains("Passwords must have at least one uppercase", StringComparison.OrdinalIgnoreCase))
                        {
                            ModelState.AddModelError("CurrentPassword", "La contraseña debe tener al menos una letra mayúscula ('A'-'Z').");
                        }
                        else
                        {
                            ModelState.AddModelError("CurrentPassword", error);
                        }
                    }
                    return View("Edit", vm);
                }

                await _userService.SignOutAsync();
                HttpContext.Session.Clear();
                TempData["SuccessMessage"] = "Su contraseña fue restablecida correctamente. Ya puede iniciar sesión.";
                return RedirectToAction("Login", "User");
            }

            var origin = $"{Request.Scheme}://{Request.Host}";
            var saveDto = new SaveUserDto
            {
                Id = userId,
                FirstName = vm.FirstName.Trim(),
                LastName = vm.LastName.Trim(),
                Email = vm.Email,
                UserName = vm.UserName,
                Password = "",
                PhoneNumber = vm.PhoneNumber,
                PhotoUrl = vm.PhotoUrl,
                CoverPhotoUrl = vm.CoverPhotoUrl,
                Role = currentUser.Role
            };

            var updateResponse = await _userService.EditUser(saveDto, origin, false);

            if (updateResponse.HasError)
            {
                foreach (var error in updateResponse.Errors)
                {
                    if (error.Contains("username", StringComparison.OrdinalIgnoreCase) && error.Contains("taken", StringComparison.OrdinalIgnoreCase))
                    {
                        ModelState.AddModelError("UserName", "Este nombre de usuario ya se encuentra registrado.");
                    }
                    else if (error.Contains("email", StringComparison.OrdinalIgnoreCase) && error.Contains("taken", StringComparison.OrdinalIgnoreCase))
                    {
                        ModelState.AddModelError("Email", "Este correo electrónico ya se encuentra registrado.");
                    }
                    else if (error.Contains("Passwords must have at least one non alphanumeric character", StringComparison.OrdinalIgnoreCase))
                    {
                        ModelState.AddModelError("", "La contraseña debe tener al menos un carácter no alfanumérico.");
                    }
                    else if (error.Contains("Passwords must have at least one uppercase", StringComparison.OrdinalIgnoreCase))
                    {
                        ModelState.AddModelError("", "La contraseña debe tener al menos una letra mayúscula ('A'-'Z').");
                    }
                    else
                    {
                        ModelState.AddModelError("", error);
                    }
                }
                return View("Edit", vm);
            }

            TempData["SuccessMessage"] = "Su perfil fue actualizado correctamente.";
            return RedirectToAction("Index");
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
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "profiles");
            Directory.CreateDirectory(uploadsFolder);

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var fileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return $"/uploads/profiles/{fileName}";
        }

        #endregion
    }
}
