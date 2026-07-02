using System;

namespace LinkUpPro.Core.Application.DTOs.UserDTO
{
    public class UserDTO
    {
        public required string Id { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string UserName { get; set; }
        public required string Email { get; set; }
        public required string PhoneNumber { get; set; }
        public string? PhotoUrl { get; set; }
        public string? CoverPhotoUrl { get; set; }
        public bool IsActive { get; set; }
        public bool IsVerified { get; set; }
        public required string Role { get; set; }
    }
}
