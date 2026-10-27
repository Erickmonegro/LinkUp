using LinkUpPro.Core.Application.DTOs.Email;
using LinkUpPro.Core.Application.DTOs.User;
using LinkUpPro.Core.Application.DTOs.UserDTO;
using LinkUpPro.Core.Application.Interfaces;
using LinkUpPro.Infrastructure.Identity.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace LinkUpPro.Infrastructure.Identity.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IEmailService _emailService;
        private static readonly System.Collections.Concurrent.ConcurrentDictionary<string, DateTime> _activationCooldowns = new(StringComparer.OrdinalIgnoreCase);

        public UserService(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, IEmailService emailService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailService = emailService;
        }

        public async Task<LoginResponseDto> AuthenticateAsync(LoginDto loginDto)
        {
            LoginResponseDto response = new()
            {
                Email = "",
                Id = "",
                LastName = "",
                FirstName = "",
                UserName = "",
                HasError = false,
                Errors = []
            };

            var user = await _userManager.FindByNameAsync(loginDto.UserName)
                       ?? await _userManager.FindByEmailAsync(loginDto.UserName);

            if (user == null)
            {
                response.HasError = true;
                // Mensaje genérico unificado para evitar enumeración de usuarios
                response.Errors.Add("El nombre de usuario o la contraseña son incorrectos.");
                return response;
            }

            if (!user.EmailConfirmed)
            {
                response.HasError = true;
                response.Errors.Add("Su cuenta se encuentra inactiva. Debe activarla mediante el enlace enviado a su correo electrónico.");
                return response;
            }

            var result = await _signInManager.PasswordSignInAsync(user.UserName!, loginDto.Password, loginDto.RememberMe, true);

            if (!result.Succeeded)
            {
                response.HasError = true;
                if (result.IsLockedOut)
                {
                    response.Errors.Add("La cuenta se encuentra bloqueada temporalmente debido a varios intentos fallidos. Inténtelo nuevamente en 15 minutos o restablezca su contraseña.");
                }
                else
                {
                    response.Errors.Add("El nombre de usuario o la contraseña son incorrectos.");
                }
                return response;
            }

            var rolesList = await _userManager.GetRolesAsync(user);

            response.Id = user.Id;
            response.Email = user.Email!;
            response.UserName = user.UserName!;
            response.FirstName = user.Name;
            response.LastName = user.LastName;
            response.IsVerified = user.EmailConfirmed;
            response.Roles = rolesList.ToList();

            return response;
        }

        public async Task SignOutAsync()
        {
            await _signInManager.SignOutAsync();
        }

        public async Task<RegisterResponseDto> RegisterUser(SaveUserDto saveDto, string origin)
        {
            RegisterResponseDto response = new()
            {
                Email = "",
                Id = "",
                LastName = "",
                FirstName = "",
                UserName = "",
                HasError = false,
                Errors = []
            };

            var userWithSameUserName = await _userManager.FindByNameAsync(saveDto.UserName);
            if (userWithSameUserName != null)
            {
                response.HasError = true;
                response.Errors.Add("Este nombre de usuario ya se encuentra registrado.");
                return response;
            }

            var userWithSameEmail = await _userManager.FindByEmailAsync(saveDto.Email);
            if (userWithSameEmail != null)
            {
                response.HasError = true;
                response.Errors.Add("Este correo electrónico ya se encuentra registrado.");
                return response;
            }

            AppUser user = new AppUser()
            {
                Name = saveDto.FirstName,
                LastName = saveDto.LastName,
                Email = saveDto.Email,
                UserName = saveDto.UserName,
                ProfilePhoto = saveDto.PhotoUrl ?? string.Empty,
                CoverPhoto = saveDto.CoverPhotoUrl,
                EmailConfirmed = false,
                PhoneNumber = saveDto.PhoneNumber ?? string.Empty
            };

            var result = await _userManager.CreateAsync(user, saveDto.Password);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, saveDto.Role);
                string verificationUri = await GetVerificationEmailUri(user, origin);
                await _emailService.SendAsync(new EmailRequestDTO()
                {
                    To = saveDto.Email,
                    Subject = "Activación de cuenta - LinkUp Pro",
                    HtmlBody = $@"<div style='font-family: Arial, sans-serif; max-width: 600px; margin: auto; padding: 20px; border: 1px solid #e2e8f0; border-radius: 12px;'>
                        <h2 style='color: #060396;'>¡Bienvenido a LinkUp Pro!</h2>
                        <p>Hola <strong>{saveDto.FirstName}</strong>,</p>
                        <p>Su cuenta fue creada correctamente. Para activarla e iniciar sesión, haga clic en el siguiente enlace:</p>
                        <p style='text-align: center; margin: 30px 0;'>
                            <a href='{verificationUri}' style='background-color: #060396; color: white; padding: 14px 28px; text-decoration: none; border-radius: 8px; font-weight: bold; display: inline-block;'>Activar mi cuenta</a>
                        </p>
                        <p style='color: #64748b; font-size: 13px;'>Si no puede hacer clic en el botón, copie y pegue este enlace en su navegador:<br><a href='{verificationUri}'>{verificationUri}</a></p>
                    </div>"
                });

                response.Id = user.Id;
                response.Email = user.Email!;
                response.UserName = user.UserName!;
                response.FirstName = user.Name;
                response.LastName = user.LastName;

                return response;
            }
            else
            {
                response.HasError = true;
                response.Errors.AddRange(FormatErrors(result.Errors));
                return response;
            }
        }

        public async Task<EditResponseDto> EditUser(SaveUserDto saveDto, string origin, bool? isCreated = false)
        {
            bool isNotcreated = !isCreated ?? false;
            EditResponseDto response = new()
            {
                Email = "",
                Id = "",
                LastName = "",
                FirstName = "",
                UserName = "",
                HasError = false,
                Errors = []
            };

            var userWithSameUserName = await _userManager.Users.FirstOrDefaultAsync(w => w.UserName == saveDto.UserName && w.Id != saveDto.Id);
            if (userWithSameUserName != null)
            {
                response.HasError = true;
                response.Errors.Add("Este nombre de usuario ya se encuentra registrado.");
                return response;
            }

            var userWithSameEmail = await _userManager.Users.FirstOrDefaultAsync(w => w.Email == saveDto.Email && w.Id != saveDto.Id);
            if (userWithSameEmail != null)
            {
                response.HasError = true;
                response.Errors.Add("Este correo electrónico ya se encuentra registrado.");
                return response;
            }

            var user = await _userManager.FindByIdAsync(saveDto.Id);

            if (user == null)
            {
                response.HasError = true;
                response.Errors.Add("No existe una cuenta registrada para este usuario.");
                return response;
            }

            user.Name = saveDto.FirstName;
            user.LastName = saveDto.LastName;
            user.UserName = saveDto.UserName;
            user.ProfilePhoto = string.IsNullOrWhiteSpace(saveDto.PhotoUrl) ? user.ProfilePhoto : saveDto.PhotoUrl;
            user.CoverPhoto = string.IsNullOrWhiteSpace(saveDto.CoverPhotoUrl) ? user.CoverPhoto : saveDto.CoverPhotoUrl;
            user.EmailConfirmed = user.EmailConfirmed && user.Email == saveDto.Email;
            user.Email = saveDto.Email;
            if (!string.IsNullOrWhiteSpace(saveDto.PhoneNumber))
            {
                user.PhoneNumber = saveDto.PhoneNumber;
            }

            if (!string.IsNullOrWhiteSpace(saveDto.Password) && isNotcreated)
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var resultChange = await _userManager.ResetPasswordAsync(user, token, saveDto.Password);

                if (resultChange != null && !resultChange.Succeeded)
                {
                    response.HasError = true;
                    response.Errors.AddRange(FormatErrors(resultChange.Errors));
                    return response;
                }
                await _userManager.ResetAccessFailedCountAsync(user);
                await _userManager.SetLockoutEndDateAsync(user, null);
            }

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                var rolesList = await _userManager.GetRolesAsync(user);
                await _userManager.RemoveFromRolesAsync(user, rolesList.ToList());
                await _userManager.AddToRoleAsync(user, saveDto.Role);

                if (!user.EmailConfirmed && isNotcreated)
                {
                    string verificationUri = await GetVerificationEmailUri(user, origin);
                    await _emailService.SendAsync(new EmailRequestDTO()
                    {
                        To = saveDto.Email,
                        Subject = "Activación de cuenta - LinkUp Pro",
                        HtmlBody = $@"<div style='font-family: Arial, sans-serif; max-width: 600px; margin: auto; padding: 20px; border: 1px solid #e2e8f0; border-radius: 12px;'>
                            <h2 style='color: #060396;'>Activación de cuenta</h2>
                            <p>Hola <strong>{saveDto.FirstName}</strong>,</p>
                            <p>Para activar tu cuenta, haz clic en el siguiente botón:</p>
                            <p style='text-align: center; margin: 30px 0;'>
                                <a href='{verificationUri}' style='background-color: #060396; color: white; padding: 14px 28px; text-decoration: none; border-radius: 8px; font-weight: bold; display: inline-block;'>Activar mi cuenta</a>
                            </p>
                        </div>"
                    });
                }

                response.Id = user.Id;
                response.Email = user.Email!;
                response.UserName = user.UserName!;
                response.FirstName = user.Name;
                response.LastName = user.LastName;
                response.IsVerified = user.EmailConfirmed;

                return response;
            }
            else
            {
                response.HasError = true;
                response.Errors.AddRange(FormatErrors(result.Errors));
                return response;
            }
        }

        public async Task<UserResponseDto> ForgotPasswordAsync(ForgotPasswordRequestDto request)
        {
            UserResponseDto response = new() { HasError = false, Errors = [] };

            var user = await _userManager.FindByNameAsync(request.UserName) 
                       ?? await _userManager.FindByEmailAsync(request.UserName);

            if (user == null)
            {
                // No revelar si existe o no
                return response;
            }

            var resetUri = await GetResetPasswordUri(user, request.Origin);

            await _emailService.SendAsync(new EmailRequestDTO()
            {
                To = user.Email!,
                HtmlBody = $@"<div style='font-family: Arial, sans-serif; max-width: 600px; margin: auto; padding: 20px; border: 1px solid #e2e8f0; border-radius: 12px;'>
                    <h2 style='color: #060396;'>Recuperación de Contraseña</h2>
                    <p>Hola <strong>{user.Name}</strong>,</p>
                    <p>Hemos recibido una solicitud para restablecer la contraseña de su cuenta. Visite el siguiente enlace para crear una nueva contraseña:</p>
                    <p style='text-align: center; margin: 30px 0;'>
                        <a href='{resetUri}' style='background-color: #060396; color: white; padding: 14px 28px; text-decoration: none; border-radius: 8px; font-weight: bold; display: inline-block;'>Restablecer mi contraseña</a>
                    </p>
                    <p style='color: #64748b; font-size: 13px;'>Este enlace tiene una vigencia máxima de una hora. Si usted no solicitó este cambio, puede ignorar este mensaje.</p>
                </div>",
                Subject = "Restablecer contraseña - LinkUp Pro"
            });

            return response;
        }

        public async Task<UserResponseDto> ResetPasswordAsync(ResetPasswordRequestDto request)
        {
            UserResponseDto response = new() { HasError = false, Errors = [] };

            var user = await _userManager.FindByEmailAsync(request.Email);

            if (user == null)
            {
                response.HasError = true;
                response.Errors.Add("El enlace para restablecer la contraseña no es válido o ya fue utilizado.");
                return response;
            }

            var token = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(request.Token));
            var result = await _userManager.ResetPasswordAsync(user, token, request.Password);
            if (!result.Succeeded)
            {
                response.HasError = true;
                response.Errors.AddRange(FormatErrors(result.Errors));
                return response;
            }

            // REGLA CRÍTICA: NO cambiar EmailConfirmed (preservar el estado de activación)
            await _userManager.ResetAccessFailedCountAsync(user);
            await _userManager.SetLockoutEndDateAsync(user, null);
            await _userManager.UpdateSecurityStampAsync(user); // Invalida todas las sesiones previas

            return response;
        }

        public async Task<UserResponseDto> DeleteAsync(string id)
        {
            UserResponseDto response = new() { HasError = false, Errors = [] };
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                response.HasError = true;
                response.Errors.Add("No existe una cuenta registrada para este usuario.");
                return response;
            }

            await _userManager.DeleteAsync(user);
            return response;
        }

        public async Task<UserDTO?> GetUserByEmail(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                return null;
            }

            var rolesList = await _userManager.GetRolesAsync(user);

            var userDto = new UserDTO()
            {
                Id = user.Id,
                Email = user.Email!,
                LastName = user.LastName,
                FirstName = user.Name,
                UserName = user.UserName!,
                PhotoUrl = user.ProfilePhoto,
                CoverPhotoUrl = user.CoverPhoto,
                PhoneNumber = user.PhoneNumber ?? string.Empty,
                IsVerified = user.EmailConfirmed,
                Role = rolesList.FirstOrDefault() ?? "",
                IsActive = user.EmailConfirmed && (!user.LockoutEnd.HasValue || user.LockoutEnd <= DateTimeOffset.UtcNow)
            };

            return userDto;
        }

        public async Task<UserDTO?> GetUserById(string Id)
        {
            var user = await _userManager.FindByIdAsync(Id);

            if (user == null)
            {
                return null;
            }

            var rolesList = await _userManager.GetRolesAsync(user);

            var userDto = new UserDTO()
            {
                Id = user.Id,
                Email = user.Email!,
                LastName = user.LastName,
                FirstName = user.Name,
                UserName = user.UserName!,
                PhotoUrl = user.ProfilePhoto,
                CoverPhotoUrl = user.CoverPhoto,
                PhoneNumber = user.PhoneNumber ?? string.Empty,
                IsVerified = user.EmailConfirmed,
                Role = rolesList.FirstOrDefault() ?? "",
                IsActive = user.EmailConfirmed && (!user.LockoutEnd.HasValue || user.LockoutEnd <= DateTimeOffset.UtcNow)
            };

            return userDto;
        }

        public async Task<UserDTO?> GetUserByUserName(string userName)
        {
            var user = await _userManager.FindByNameAsync(userName);

            if (user == null)
            {
                return null;
            }

            var rolesList = await _userManager.GetRolesAsync(user);

            var userDto = new UserDTO()
            {
                Id = user.Id,
                Email = user.Email!,
                LastName = user.LastName,
                FirstName = user.Name,
                UserName = user.UserName!,
                PhotoUrl = user.ProfilePhoto,
                CoverPhotoUrl = user.CoverPhoto,
                PhoneNumber = user.PhoneNumber ?? string.Empty,
                IsVerified = user.EmailConfirmed,
                Role = rolesList.FirstOrDefault() ?? "",
                IsActive = user.EmailConfirmed && (!user.LockoutEnd.HasValue || user.LockoutEnd <= DateTimeOffset.UtcNow)
            };

            return userDto;
        }

        public async Task<List<UserDTO>> GetAllUser(bool? isActive = true)
        {
            List<UserDTO> listUsersDtos = [];

            var users = _userManager.Users;

            if (isActive != null && isActive == true)
            {
                users = users.Where(w => w.EmailConfirmed);
            }

            var listUser = await users.ToListAsync();

            foreach (var item in listUser)
            {
                var roleList = await _userManager.GetRolesAsync(item);

                listUsersDtos.Add(new UserDTO()
                {
                    Id = item.Id,
                    Email = item.Email!,
                    LastName = item.LastName,
                    FirstName = item.Name,
                    UserName = item.UserName!,
                    PhotoUrl = item.ProfilePhoto,
                    CoverPhotoUrl = item.CoverPhoto,
                    PhoneNumber = item.PhoneNumber ?? string.Empty,
                    IsVerified = item.EmailConfirmed,
                    Role = roleList.FirstOrDefault() ?? "",
                    IsActive = item.EmailConfirmed && (!item.LockoutEnd.HasValue || item.LockoutEnd <= DateTimeOffset.UtcNow)
                });
            }

            return listUsersDtos;
        }

        public async Task<string> ConfirmAccountAsync(string userId, string token)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return "No existe una cuenta registrada para este usuario.";
            }

            token = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));
            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (result.Succeeded)
            {
                await _userManager.UpdateSecurityStampAsync(user);
                return $"Cuenta confirmada para {user.Email}. Ya puedes iniciar sesión.";
            }
            else
            {
                return "El enlace de activación no es válido o ya fue utilizado.";
            }
        }

        public async Task<UserResponseDto> ChangePasswordAsync(string userId, string currentPassword, string newPassword)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return new UserResponseDto { HasError = true, Errors = new List<string> { "No existe una cuenta registrada para este usuario." } };
            }

            var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
            if (!result.Succeeded)
            {
                return new UserResponseDto { HasError = true, Errors = FormatErrors(result.Errors) };
            }

            await _userManager.UpdateSecurityStampAsync(user);
            return new UserResponseDto { HasError = false, Errors = new List<string>() };
        }

        public async Task<UserResponseDto> ResendActivationAsync(string userName, string origin)
        {
            var response = new UserResponseDto { Errors = new List<string>() };
            var cacheKey = userName.Trim();
            if (_activationCooldowns.TryGetValue(cacheKey, out var lastRequestTime))
            {
                if ((DateTime.UtcNow - lastRequestTime).TotalMinutes < 5)
                {
                    response.HasError = true;
                    response.Errors.Add("No puede realizar múltiples solicitudes de código de activación consecutivas en un período menor a cinco minutos. Por favor espere e inténtelo de nuevo.");
                    return response;
                }
            }

            _activationCooldowns[cacheKey] = DateTime.UtcNow;

            var appUser = await _userManager.FindByNameAsync(userName.Trim()) 
                          ?? await _userManager.FindByEmailAsync(userName.Trim());

            // Solo reenviar si existe y no ha sido activada aún
            if (appUser != null && !appUser.EmailConfirmed)
            {
                // Invalidar tokens de activación anteriores regenerando SecurityStamp
                await _userManager.UpdateSecurityStampAsync(appUser);

                string verificationUri = await GetVerificationEmailUri(appUser, origin);
                await _emailService.SendAsync(new EmailRequestDTO
                {
                    To = appUser.Email!,
                    Subject = "Activación de cuenta - LinkUp Pro",
                    HtmlBody = $@"<div style='font-family: Arial, sans-serif; max-width: 600px; margin: auto; padding: 20px; border: 1px solid #e2e8f0; border-radius: 12px;'>
                        <h2 style='color: #060396;'>Enlace de Activación de Cuenta</h2>
                        <p>Hola <strong>{appUser.Name}</strong>,</p>
                        <p>Ha solicitado el reenvío del enlace de activación para su cuenta en LinkUp Pro. Para activarla, haga clic en el siguiente enlace:</p>
                        <p style='text-align: center; margin: 30px 0;'>
                            <a href='{verificationUri}' style='background-color: #060396; color: white; padding: 14px 28px; text-decoration: none; border-radius: 8px; font-weight: bold; display: inline-block;'>Activar mi cuenta</a>
                        </p>
                        <p style='color: #64748b; font-size: 13px;'>Este enlace tiene una vigencia máxima de 24 horas.<br><a href='{verificationUri}'>{verificationUri}</a></p>
                    </div>"
                });
            }

            return response;
        }

        private static List<string> FormatErrors(IEnumerable<IdentityError> errors)
        {
            var list = new List<string>();
            foreach (var err in errors)
            {
                if (err.Description.Contains("alphanumeric", StringComparison.OrdinalIgnoreCase))
                    list.Add("La contraseña debe tener al menos un carácter especial.");
                else if (err.Description.Contains("uppercase", StringComparison.OrdinalIgnoreCase))
                    list.Add("La contraseña debe tener al menos una letra mayúscula.");
                else if (err.Description.Contains("lowercase", StringComparison.OrdinalIgnoreCase))
                    list.Add("La contraseña debe tener al menos una letra minúscula.");
                else if (err.Description.Contains("digit", StringComparison.OrdinalIgnoreCase))
                    list.Add("La contraseña debe tener al menos un número.");
                else if (err.Description.Contains("PasswordTooShort", StringComparison.OrdinalIgnoreCase) || err.Description.Contains("length", StringComparison.OrdinalIgnoreCase))
                    list.Add("La contraseña debe tener al menos ocho caracteres.");
                else if (err.Code == "PasswordMismatch")
                    list.Add("La contraseña actual es incorrecta.");
                else
                    list.Add(err.Description);
            }
            return list;
        }

        #region "Private methods"

        private async Task<string> GetVerificationEmailUri(AppUser user, string origin)
        {
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            token = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
            var route = "User/ConfirmEmail";
            var completeUrl = new Uri(string.Concat(origin, "/", route));
            var verificationUri = QueryHelpers.AddQueryString(completeUrl.ToString(), "userId", user.Id);
            verificationUri = QueryHelpers.AddQueryString(verificationUri.ToString(), "token", token);

            return verificationUri;
        }

        private async Task<string> GetResetPasswordUri(AppUser user, string origin)
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            token = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
            var route = "Login/ResetPassword";
            var completeUrl = new Uri(string.Concat(origin, "/", route));
            var resetUri = QueryHelpers.AddQueryString(completeUrl.ToString(), "userId", user.Id);
            resetUri = QueryHelpers.AddQueryString(resetUri.ToString(), "token", token);

            return resetUri;
        }

        #endregion
    }
}
