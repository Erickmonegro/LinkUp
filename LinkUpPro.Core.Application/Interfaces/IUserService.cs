using LinkUpPro.Core.Application.DTOs.User;
using LinkUpPro.Core.Application.DTOs.UserDTO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LinkUpPro.Core.Application.Interfaces
{
    public interface IUserService
    {
        Task<LoginResponseDto> AuthenticateAsync(LoginDto loginDto);
        Task<string> ConfirmAccountAsync(string userId, string token);
        Task<UserResponseDto> DeleteAsync(string id);
        Task<EditResponseDto> EditUser(SaveUserDto saveDto, string origin, bool? isCreated = false);
        Task<UserResponseDto> ForgotPasswordAsync(ForgotPasswordRequestDto request);
        Task<List<UserDTO>> GetAllUser(bool? isActive = true);
        Task<UserDTO?> GetUserByEmail(string email);
        Task<UserDTO?> GetUserByUserName(string userName);
        Task<UserDTO?> GetUserById(string Id);
        Task<RegisterResponseDto> RegisterUser(SaveUserDto saveDto, string origin);
        Task<UserResponseDto> ResetPasswordAsync(ResetPasswordRequestDto request);
        Task<UserResponseDto> ResendActivationAsync(string userName, string origin);
        Task<UserResponseDto> ChangePasswordAsync(string userId, string currentPassword, string newPassword);
        Task SignOutAsync();
    }
}
