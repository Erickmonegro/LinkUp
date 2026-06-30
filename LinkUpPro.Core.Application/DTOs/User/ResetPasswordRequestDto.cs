namespace LinkUpPro.Core.Application.DTOs.User
{
    public class ResetPasswordRequestDto
    {
        public required string Email { get; set; }
        public required string Token { get; set; }
        public required string Password { get; set; }    
    }
}
