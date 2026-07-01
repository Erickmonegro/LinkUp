using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LinkUpPro.Core.Application.ViewModels.FriendRequest
{
    public class FriendRequestListViewModel
    {
        [Display(Name = "Solicitudes Pendientes")]
        public int PendingCount { get; set; }

        public List<FriendRequestViewModel> ReceivedRequests { get; set; } = new List<FriendRequestViewModel>();

        public List<FriendRequestViewModel> SentRequests { get; set; } = new List<FriendRequestViewModel>();
    }
}
