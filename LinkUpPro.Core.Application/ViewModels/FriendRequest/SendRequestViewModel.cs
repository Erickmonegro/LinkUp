using LinkUpPro.Core.Application.ViewModels.Friendship;
using System.ComponentModel.DataAnnotations;

namespace LinkUpPro.Core.Application.ViewModels.FriendRequest
{
    public class SendRequestViewModel
    {
        [Display(Name = "Buscar amigo (nombre o usuario)")]
        public string SearchTerm { get; set; } = string.Empty;

        public List<FriendViewModel> AvailableUsers { get; set; } = new List<FriendViewModel>();

        [Required(ErrorMessage = "Debe seleccionar un usuario para enviar la solicitud.")]
        public string? SelectedUserId { get; set; }

        public List<FriendViewModel> CurrentFriends { get; set; } = new List<FriendViewModel>();
    }
}
