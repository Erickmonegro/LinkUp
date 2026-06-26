using System;

namespace LinkUpPro.Core.Application.DTOs.UserDTO
{
    public class UserDTO : BasicDTO<string>
    {
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string UserName { get; set; }
        public required string Email { get; set; }
        public required string PhoneNumber { get; set; }
        public string? PhotoUrl { get; set; }
        public bool IsActive { get; set; }
    }
}
