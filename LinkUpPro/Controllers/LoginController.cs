using AutoMapper;
using LinkUpPro.Core.Application.DTOs.User;
using LinkUpPro.Core.Application.Interfaces;
using LinkUpPro.Core.Application.ViewModels.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace LinkUpPro.Controllers
{
    public class LoginController : Controller
    {
        private readonly IUserService _userService;
        private readonly IMapper _mapper;

        public LoginController(IUserService userService, IMapper mapper)
        {
            _userService = userService;
            _mapper = mapper;
        }

        #region Login

        [AllowAnonymous]
        public IActionResult Login([FromQuery] bool timeout = false)
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }

            ViewData["HideNav"] = true;
            if (timeout || TempData["InactivityTimeout"] != null)
            {
                ViewBag.InactivityMessage = "Su sesión finalizó por inactividad. Inicie sesión nuevamente para continuar.";
            }

            return View("~/Views/Home/Index.cshtml", new LoginViewModel { UserName = "", Password = "" });
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel vm)
        {
            ViewData["HideNav"] = true;

            if (!ModelState.IsValid)
            {
                return View("~/Views/Home/Index.cshtml", vm);
            }

            var loginDto = _mapper.Map<LoginDto>(vm);

            var response = await _userService.AuthenticateAsync(loginDto);

            if (response.HasError)
            {
                foreach (var error in response.Errors)
                {
                    ModelState.AddModelError("", error);
                }
                return View("~/Views/Home/Index.cshtml", vm);
            }

            TempData["JustLoggedIn"] = true;
            return RedirectToAction("Index", "Home");
        }
        #endregion

        #region Logout

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _userService.SignOutAsync();
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        #endregion

        #region Forgot Password

        [AllowAnonymous]
        public IActionResult ForgotPassword()
        {
            ViewData["HideNav"] = true;
            return View(new ForgotPasswordRequestViewModel { UserName = "" });
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordRequestViewModel vm)
        {
            ViewData["HideNav"] = true;

            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            var origin = $"{Request.Scheme}://{Request.Host}";
            var dto = _mapper.Map<ForgotPasswordRequestDto>(vm);
            dto.UserName = vm.UserName.Trim();
            dto.Origin = origin;

            await _userService.ForgotPasswordAsync(dto);

            ViewBag.Message = "Si el nombre de usuario corresponde a una cuenta registrada, recibirá un enlace para restablecer su contraseña.";
            return View("ForgotPasswordResult");
        }

        [AllowAnonymous]
        public IActionResult ForgotPasswordResult()
        {
            ViewData["HideNav"] = true;
            return View();
        }

        #endregion

        #region Reset Password

        [AllowAnonymous]
        public IActionResult ResetPassword(string userId, string token)
        {
            ViewData["HideNav"] = true;

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
            {
                ViewBag.Error = "El enlace para restablecer la contraseña no es válido o ya fue utilizado.";
                return View("ResetPasswordError");
            }

            return View(new ResetPasswordRequestViewModel
            {
                UserId = userId,
                Token = token,
                Password = "",
                ConfirmPassword = ""
            });
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordRequestViewModel vm)
        {
            ViewData["HideNav"] = true;

            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            var user = await _userService.GetUserById(vm.UserId);
            if (user == null)
            {
                ViewBag.Error = "El enlace para restablecer la contraseña no es válido o ya fue utilizado.";
                return View("ResetPasswordError");
            }

            var dto = _mapper.Map<ResetPasswordRequestDto>(vm);
            dto.Email = user.Email;

            var response = await _userService.ResetPasswordAsync(dto);

            if (response.HasError)
            {
                ViewBag.HasError = true;
                ViewBag.Errors = response.Errors;
                return View(vm);
            }

            TempData["SuccessMessage"] = "Su contraseña fue restablecida correctamente. Ya puede iniciar sesión.";
            return RedirectToAction("Login");
        }

        #endregion

        #region Access Denied

        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            ViewData["HideNav"] = true;
            return View();
        }

        #endregion
    }
}
