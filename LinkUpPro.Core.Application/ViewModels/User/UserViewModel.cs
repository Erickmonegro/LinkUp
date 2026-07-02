using System.ComponentModel.DataAnnotations;

namespace LinkUpPro.Core.Application.ViewModels.User
{
    public class UserViewModel 
    {
        public required string Id { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string Email { get; set; }
        public required string UserName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? PhotoUrl { get; set; }
        public string? CoverPhotoUrl { get; set; }
        public required string Role { get; set; }
    }
}
