using System.ComponentModel.DataAnnotations;

namespace LinkUpPro.Core.Application.ViewModels.Friendship
{
    public class FriendProfileViewModel
    {
        [Required(ErrorMessage = "El Id de usuario es requerido.")]
        public string UserId { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre completo es requerido.")]
        [Display(Name = "Nombre Completo")]
        public string FullName { get; set; } = string.Empty;

        public string? ProfilePhoto { get; set; }
        public string? CoverPhoto { get; set; }

        [Required(ErrorMessage = "El nombre de usuario es requerido.")]
        [Display(Name = "Nombre de Usuario")]
        public string UserName { get; set; } = string.Empty;

        [Required(ErrorMessage = "El teléfono es requerido.")]
        [Display(Name = "Teléfono")]
        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "El correo electrónico es requerido.")]
        [EmailAddress(ErrorMessage = "El correo electrónico no es válido.")]
        [Display(Name = "Correo Electrónico")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Total de Amigos")]
        public int TotalFriends { get; set; }

        [Display(Name = "Amigos en común")]
        public int MutualFriendsCount { get; set; }

        public bool IsFriend { get; set; }

        public List<FriendViewModel> MutualFriends { get; set; } = new List<FriendViewModel>();
    }
}
