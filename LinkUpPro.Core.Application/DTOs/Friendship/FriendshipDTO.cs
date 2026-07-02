using System;

namespace LinkUpPro.Core.Application.DTOs.FriendshipDTO
{
    public class FriendshipDTO : BasicDTO<int>
    {
        public required string UserId1 { get; set; }
        public required string UserId2 { get; set; }
        public required string FriendId { get; set; }
        public required string FriendName { get; set; }
        public string? FriendPhotoUrl { get; set; }
        
        public int MutualFriendsCount { get; set; }
    }
}
