using LinkUpPro.Core.Application.DTOs.PostDTO;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LinkUpPro.Core.Application.ViewModels.Friendship
{
    public class FriendListViewModel
    {
        [Display(Name = "Amigos Activos")]
        public int ActiveFriendsCount { get; set; }

        [Display(Name = "Publicaciones de Amigos")]
        public int FriendPostsCount { get; set; }

        public List<FriendViewModel> Friends { get; set; } = new List<FriendViewModel>();

        public List<PostDTO> Posts { get; set; } = new List<PostDTO>();
    }
}
