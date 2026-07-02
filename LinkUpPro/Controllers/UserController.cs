using LinkUpPro.Core.Application.DTOs.User;
using LinkUpPro.Core.Application.Interfaces;
using LinkUpPro.Core.Application.ViewModels.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using LinkUpPro.Infrastructure.Identity.Entities;
using System.Security.Claims;
using System.Threading.Tasks;
using System;

namespace LinkUpPro.Controllers
{
    public class UserController : Controller
    {
        private readonly IUserService _userService;
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private static readonly System.Collections.Concurrent.ConcurrentDictionary<string, DateTime> _activationCooldowns = new(StringComparer.OrdinalIgnoreCase);

        public UserController(
            IUserService userService,
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager)
        {
            _userService = userService;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        #region Login

        [AllowAnonymous]
        public IActionResult Login()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }

            ViewData["HideNav"] = true;
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

            var loginDto = new LoginDto
            {
                UserName = vm.UserName,
                Password = vm.Password,
                RememberMe = vm.RememberMe
            };

            var response = await _userService.AuthenticateAsync(loginDto);

            if (response.HasError)
            {
                foreach (var error in response.Errors)
                {
                    // Translate common errors to Spanish per PDF
                    if (error.Contains("not active", StringComparison.OrdinalIgnoreCase))
                    {
                        ModelState.AddModelError("", "Su cuenta se encuentra inactiva. Debe activarla mediante el enlace enviado a su correo electrónico.");
                    }
                    else if (error.Contains("locked", StringComparison.OrdinalIgnoreCase))
                    {
                        ModelState.AddModelError("", "La cuenta se encuentra bloqueada temporalmente debido a varios intentos fallidos. Inténtelo nuevamente en 15 minutos o restablezca su contraseña.");
                    }
                    else
                    {
                        ModelState.AddModelError("", "El nombre de usuario o la contraseña son incorrectos.");
                    }
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

            // Validate image file
            string? photoUrl = null;
            if (vm.ProfileImageFile != null && vm.ProfileImageFile.Length > 0)
            {
                var validationError = ValidateImageFile(vm.ProfileImageFile);
                if (validationError != null)
                {
                    ModelState.AddModelError("ProfileImageFile", validationError);
                    return View(vm);
                }
                photoUrl = await SaveImageFileAsync(vm.ProfileImageFile);
            }

            var origin = $"{Request.Scheme}://{Request.Host}";

            var saveDto = new SaveUserDto
            {
                Id = "",
                FirstName = vm.FirstName.Trim(),
                LastName = vm.LastName.Trim(),
                Email = vm.Email.Trim(),
                UserName = vm.UserName.Trim(),
                Password = vm.Password,
                PhoneNumber = vm.PhoneNumber,
                PhotoUrl = photoUrl,
                Role = "StandardUser"
            };

            var response = await _userService.RegisterUser(saveDto, origin);

            if (response.HasError)
            {
                foreach (var error in response.Errors)
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
                return View(vm);
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

            if (result.Contains("confirmed", StringComparison.OrdinalIgnoreCase))
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

            var cacheKey = vm.UserName.Trim();
            if (_activationCooldowns.TryGetValue(cacheKey, out var lastRequestTime))
            {
                if ((DateTime.UtcNow - lastRequestTime).TotalMinutes < 5)
                {
                    ModelState.AddModelError("", "No puede realizar múltiples solicitudes de código de activación consecutivas en un período menor a cinco minutos. Por favor espere e inténtelo de nuevo.");
                    return View(vm);
                }
            }

            _activationCooldowns[cacheKey] = DateTime.UtcNow;

            // Always show same message regardless of whether account exists
            var user = await _userService.GetUserByUserName(vm.UserName.Trim());

            if (user != null && !user.IsVerified)
            {
                var origin = $"{Request.Scheme}://{Request.Host}";
                var saveDto = new SaveUserDto
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    UserName = user.UserName,
                    Password = "",
                    Role = user.Role
                };

                // Re-send activation email through EditUser
                await _userService.EditUser(saveDto, origin, false);
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
            var dto = new ForgotPasswordRequestDto
            {
                UserName = vm.UserName.Trim(),
                Origin = origin
            };

            await _userService.ForgotPasswordAsync(dto);

            // Always show the same message regardless of whether username exists
            ViewBag.Message = "Si el usuario o correo corresponde a una cuenta registrada, recibirá un enlace para restablecer su contraseña.";
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
                ViewBag.Error = "El enlace para restablecer la contraseña no es válido.";
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

            // Get user to find email
            var user = await _userService.GetUserById(vm.UserId);
            if (user == null)
            {
                ViewBag.Error = "El enlace para restablecer la contraseña no es válido.";
                return View("ResetPasswordError");
            }

            var dto = new ResetPasswordRequestDto
            {
                Email = user.Email,
                Token = vm.Token,
                Password = vm.Password
            };

            var response = await _userService.ResetPasswordAsync(dto);

            if (response.HasError)
            {
                foreach (var error in response.Errors)
                {
                    if (error.Contains("Passwords must have at least one non alphanumeric character", StringComparison.OrdinalIgnoreCase))
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

            // Validate real content type
            var allowedMimeTypes = new[] { "image/jpeg", "image/png", "image/webp" };
            if (!allowedMimeTypes.Contains(file.ContentType.ToLowerInvariant()))
            {
                return "El archivo seleccionado no tiene un formato de imagen válido.";
            }

            return null;
        }

        private static async Task<string> SaveImageFileAsync(IFormFile file)
        {
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "profiles");
            Directory.CreateDirectory(uploadsFolder);

            // Generate internal filename (never use user-provided name)
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
