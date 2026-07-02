using AutoMapper;
using LinkUpPro.Core.Application.DTOs.User;
using LinkUpPro.Core.Application.Interfaces;
using LinkUpPro.Core.Application.ViewModels.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace LinkUpPro.Controllers
{
    public class UserController : Controller
    {
        private readonly IUserService _userService;
        private readonly IMapper _mapper;
        private readonly IImageStorageService _imageStorageService;

        public UserController(
            IUserService userService,
            IMapper mapper,
            IImageStorageService imageStorageService)
        {
            _userService = userService;
            _mapper = mapper;
            _imageStorageService = imageStorageService;
        }

        #region Register

        [AllowAnonymous]
        public IActionResult Register()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }

            ViewData["HideNav"] = true;
            return View(new RegisterUserViewModel
            {
                FirstName = "",
                LastName = "",
                Email = "",
                UserName = "",
                Password = "",
                ConfirmPassword = "",
                PhoneNumber = ""
            });
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterUserViewModel vm)
        {
            ViewData["HideNav"] = true;

            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            var origin = $"{Request.Scheme}://{Request.Host}";

            var saveDto = _mapper.Map<SaveUserDto>(vm);
            saveDto.Id = "";
            saveDto.FirstName = vm.FirstName.Trim();
            saveDto.LastName = vm.LastName.Trim();
            saveDto.Email = vm.Email.Trim();
            saveDto.UserName = vm.UserName.Trim();
            saveDto.PhotoUrl = null;
            saveDto.Role = "StandardUser";

            var response = await _userService.RegisterUser(saveDto, origin);

            if (response.HasError)
            {
                ViewBag.HasError = true;
                ViewBag.Errors = response.Errors;
                return View(vm);
            }

            if (vm.ProfileImageFile != null && !string.IsNullOrWhiteSpace(response.Id))
            {
                saveDto.Id = response.Id;
                saveDto.PhotoUrl = await _imageStorageService.UploadImageAsync(vm.ProfileImageFile, response.Id, "Users");
                await _userService.EditUser(saveDto, origin, true);
            }

            return RedirectToAction("RegistrationSuccess");
        }

        [AllowAnonymous]
        public IActionResult RegistrationSuccess()
        {
            ViewData["HideNav"] = true;
            return View();
        }

        #endregion

        #region Confirm Email

        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            ViewData["HideNav"] = true;

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
            {
                ViewBag.Message = "El enlace de activación no es válido o ya fue utilizado.";
                ViewBag.IsSuccess = false;
                return View();
            }

            var result = await _userService.ConfirmAccountAsync(userId, token);

            if (result.Contains("confirm", StringComparison.OrdinalIgnoreCase))
            {
                ViewBag.Message = "Su cuenta fue activada correctamente. Ya puede iniciar sesión.";
                ViewBag.IsSuccess = true;
            }
            else
            {
                ViewBag.Message = "El enlace de activación no es válido o ya fue utilizado.";
                ViewBag.IsSuccess = false;
            }

            return View();
        }

        #endregion

        #region Resend Activation

        [AllowAnonymous]
        public IActionResult ResendActivation()
        {
            ViewData["HideNav"] = true;
            return View(new ResendActivationViewModel { UserName = "" });
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResendActivation(ResendActivationViewModel vm)
        {
            ViewData["HideNav"] = true;

            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            var origin = $"{Request.Scheme}://{Request.Host}";
            var response = await _userService.ResendActivationAsync(vm.UserName.Trim(), origin);
            if (response.HasError)
            {
                ViewBag.HasError = true;
                ViewBag.Errors = response.Errors;
                return View(vm);
            }

            ViewBag.Message = "Si la cuenta existe y todavía no ha sido activada, recibirá un nuevo enlace de activación.";
            return View("ResendActivationResult");
        }

        [AllowAnonymous]
        public IActionResult ResendActivationResult()
        {
            ViewData["HideNav"] = true;
            return View();
        }

        #endregion


    }
}
