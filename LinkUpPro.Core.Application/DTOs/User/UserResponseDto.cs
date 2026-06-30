using System.Collections.Generic;

namespace LinkUpPro.Core.Application.DTOs.User
{
    public class UserResponseDto
    {
        public bool HasError { get; set; }
        public required List<string> Errors { get; set; }
    }
}
