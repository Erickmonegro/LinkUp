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
                return RedirectToAction("AccessDenied", "Login");
            }

            var currentUser = await _userService.GetUserById(userId);
            if (currentUser == null) return RedirectToAction("Login", "Login");

            vm.UserName = currentUser.UserName;
            vm.Email = currentUser.Email;

            if (!ModelState.IsValid)
            {
                vm.PhotoUrl = currentUser.PhotoUrl;
                vm.CoverPhotoUrl = currentUser.CoverPhotoUrl;
                return View("Edit", vm);
            }

            vm.PhotoUrl = await _imageStorageService.UploadImageAsync(vm.ProfileImageFile, userId, "Users", true, currentUser.PhotoUrl);
            vm.CoverPhotoUrl = await _imageStorageService.UploadImageAsync(vm.CoverImageFile, userId, "Covers", true, currentUser.CoverPhotoUrl);

            bool isChangingPassword = !string.IsNullOrEmpty(vm.Password);

            if (isChangingPassword)
            {
                var passResponse = await _userService.ChangePasswordAsync(userId, vm.CurrentPassword!, vm.Password!);
                if (passResponse.HasError)
                {
                    ViewBag.HasError = true;
                    ViewBag.Errors = passResponse.Errors;
                    return View("Edit", vm);
                }

                await _userService.SignOutAsync();
                HttpContext.Session.Clear();
                TempData["SuccessMessage"] = "Su contraseña fue restablecida correctamente. Ya puede iniciar sesión.";
                return RedirectToAction("Login", "Login");
            }

            var origin = $"{Request.Scheme}://{Request.Host}";
            var saveDto = _mapper.Map<SaveUserDto>(vm);
            saveDto.Id = userId;
            saveDto.FirstName = vm.FirstName.Trim();
            saveDto.LastName = vm.LastName.Trim();
            saveDto.Password = "";
            saveDto.Role = currentUser.Role;

            var updateResponse = await _userService.EditUser(saveDto, origin, false);

            if (updateResponse.HasError)
            {
                ViewBag.HasError = true;
                ViewBag.Errors = updateResponse.Errors;
                return View("Edit", vm);
            }

            TempData["SuccessMessage"] = "Su perfil fue actualizado correctamente.";
            return RedirectToAction("Index");
        }


    }
}
