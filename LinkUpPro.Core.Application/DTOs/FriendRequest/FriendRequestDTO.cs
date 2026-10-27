using LinkUpPro.Core.Domain.Common.Enums;
using System;

namespace LinkUpPro.Core.Application.DTOs.FriendRequestDTO
{
    public class FriendRequestDTO : BasicDTO<int>
    {
        public required string SenderId { get; set; }
        public required string SenderName { get; set; }
        public string? SenderUserName { get; set; }
        public string? SenderPhotoUrl { get; set; }

        public required string ReceiverId { get; set; }
        public required string ReceiverName { get; set; }
        public string? ReceiverUserName { get; set; }
        public string? ReceiverPhotoUrl { get; set; }

        public FriendRequestStatus Status { get; set; }
        public DateTime? RespondedAt { get; set; }
        
        public int MutualFriendsCount { get; set; }
    }
}
