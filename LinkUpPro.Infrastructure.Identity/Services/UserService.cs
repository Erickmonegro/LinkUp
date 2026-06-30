using LinkUpPro.Core.Application.DTOs.User;
using LinkUpPro.Core.Application.DTOs.UserDTO;
using LinkUpPro.Core.Application.Interfaces;
using LinkUpPro.Infrastructure.Identity.Entities;
using Microsoft.AspNetCore.Identity;


namespace LinkUpPro.Infrastructure.Identity.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IEmailService _emailService;

        public UserService(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, IEmailService emailService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailService = emailService;
        }
        public async Task<LoginResponseDto> AuthenticateAsync(LoginDto loginDto)
        {
            throw new NotImplementedException();
        }

        public async Task<UserResponseDto> ChangePasswordAsync(string userId, string currentPassword, string newPassword)
        {
            throw new NotImplementedException();
        }

        public async Task<string> ConfirmAccountAsync(string userId, string token)
        {
            throw new NotImplementedException();
        }

        public async Task<UserResponseDto> DeleteAsync(string id)
        {
            throw new NotImplementedException();
        }

        public async Task<EditResponseDto> EditUser(SaveUserDto saveDto, string origin, bool? isCreated = false)
        {
            throw new NotImplementedException();
        }

        public async Task<UserResponseDto> ForgotPasswordAsync(ForgotPasswordRequestDto request)
        {
            throw new NotImplementedException();
        }

        public async Task<List<UserDTO>> GetAllUser(bool? isActive = true)
        {
            throw new NotImplementedException();
        }

        public async Task<UserDTO?> GetUserByEmail(string email)
        {
            throw new NotImplementedException();
        }

        public async Task<UserDTO?> GetUserById(string Id)
        {
            throw new NotImplementedException();
        }

        public async Task<UserDTO?> GetUserByUserName(string userName)
        {
            throw new NotImplementedException();
        }

        public async Task<RegisterResponseDto> RegisterUser(SaveUserDto saveDto, string origin)
        {
            throw new NotImplementedException();
        }

        public async Task<UserResponseDto> ResetPasswordAsync(ResetPasswordRequestDto request)
        {
            throw new NotImplementedException();
        }

        public async Task SignOutAsync()
        {
            throw new NotImplementedException();
        }
    }
}
