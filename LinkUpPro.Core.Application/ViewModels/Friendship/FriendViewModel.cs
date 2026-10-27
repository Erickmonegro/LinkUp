using System.ComponentModel.DataAnnotations;

namespace LinkUpPro.Core.Application.ViewModels.Friendship
{
    public class FriendViewModel : BasicViewModel<string>
    {
        [Required(ErrorMessage = "El Id de usuario es requerido.")]
        public string UserId { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre completo es requerido.")]
        [Display(Name = "Nombre Completo")]
        public string FullName { get; set; } = string.Empty;

        public string? ProfilePhoto { get; set; }

        [Display(Name = "Nombre de Usuario")]
        public string? UserName { get; set; }

        [Display(Name = "Amigos en común")]
        public int? MutualFriendsCount { get; set; }

        public bool IsAlreadyFriend { get; set; }
    }
}
