using LinkUpPro.Core.Application.Interfaces;
using LinkUpPro.Core.Application.ViewModels.FriendRequest;
using LinkUpPro.Core.Application.ViewModels.Friendship;
using LinkUpPro.Core.Domain.Common.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace LinkUpPro.Controllers
{
    [Authorize]
    public class FriendRequestController : Controller
    {
        private readonly IFriendRequestService _friendRequestService;
        private readonly IUserService _userService;
        private readonly IFriendshipService _friendshipService;

        public FriendRequestController(
            IFriendRequestService friendRequestService,
            IUserService userService,
            IFriendshipService friendshipService)
        {
            _friendRequestService = friendRequestService;
            _userService = userService;
            _friendshipService = friendshipService;
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return RedirectToAction("Login", "User");

            var pendingReceived = await _friendRequestService.GetPendingReceivedAsync(userId);
            var sentRequests = await _friendRequestService.GetSentRequestsAsync(userId);
            var pendingCount = await _friendRequestService.GetPendingCountAsync(userId);

            // Map received requests
            var receivedVMs = pendingReceived.Select(r => new FriendRequestViewModel
            {
                Id = r.Id,
                SenderId = r.SenderId,
                SenderName = r.SenderName,
                SenderProfilePhoto = r.SenderPhotoUrl,
                ReceiverId = r.ReceiverId,
                ReceiverName = r.ReceiverName,
                ReceiverProfilePhoto = r.ReceiverPhotoUrl,
                Status = r.Status.ToString(),
                CreatedAt = r.Created,
                RespondedAt = r.RespondedAt,
                IsIncoming = true
            }).ToList();

            // Map sent requests (both pending and accepted/rejected history if not hidden)
            var sentVMs = sentRequests.Select(r => new FriendRequestViewModel
            {
                Id = r.Id,
                SenderId = r.SenderId,
                SenderName = r.SenderName,
                SenderProfilePhoto = r.SenderPhotoUrl,
                ReceiverId = r.ReceiverId,
                ReceiverName = r.ReceiverName,
                ReceiverProfilePhoto = r.ReceiverPhotoUrl,
                Status = r.Status.ToString(),
                CreatedAt = r.Created,
                RespondedAt = r.RespondedAt,
                IsIncoming = false
            }).ToList();

            var vm = new FriendRequestListViewModel
            {
                PendingCount = pendingCount,
                ReceivedRequests = receivedVMs,
                SentRequests = sentVMs
            };

            return View(vm);
        }

        public async Task<IActionResult> New(string? searchTerm)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return RedirectToAction("Login", "User");

            var currentFriends = await _friendshipService.GetFriendsAsync(userId);
            var friendIds = currentFriends.Select(f => f.FriendId).ToHashSet();

            var availableUsers = await _friendRequestService.SearchAvailableUsersAsync(userId, searchTerm ?? "");

            var availableFriendVMs = availableUsers.Select(u => new FriendViewModel
            {
                Id = "", // Not friendship id, but we map user details
                UserId = u.Id,
                FullName = $"{u.FirstName} {u.LastName}",
                ProfilePhoto = u.PhotoUrl,
                UserName = u.UserName,
                IsAlreadyFriend = friendIds.Contains(u.Id)
            }).ToList();

            var currentFriendVMs = currentFriends.Select(f => new FriendViewModel
            {
                Id = f.FriendId,
                UserId = f.FriendId,
                FullName = f.FriendName,
                ProfilePhoto = f.FriendPhotoUrl,
                MutualFriendsCount = f.MutualFriendsCount
            }).ToList();

            var vm = new SendRequestViewModel
            {
                SearchTerm = searchTerm ?? "",
                AvailableUsers = availableFriendVMs,
                CurrentFriends = currentFriendVMs
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Send(SendRequestViewModel vm)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return RedirectToAction("Login", "User");

            if (string.IsNullOrEmpty(vm.SelectedUserId))
            {
                ModelState.AddModelError("SelectedUserId", "Debe seleccionar un usuario para enviar la solicitud.");
                var currentFriends = await _friendshipService.GetFriendsAsync(userId);
                var friendIds = currentFriends.Select(f => f.FriendId).ToHashSet();
                var availableUsers = await _friendRequestService.SearchAvailableUsersAsync(userId, vm.SearchTerm);
                vm.AvailableUsers = availableUsers.Select(u => new FriendViewModel
                {
                    Id = "",
                    UserId = u.Id,
                    FullName = $"{u.FirstName} {u.LastName}",
                    ProfilePhoto = u.PhotoUrl,
                    UserName = u.UserName,
                    IsAlreadyFriend = friendIds.Contains(u.Id)
                }).ToList();
                return View("New", vm);
            }

            var result = await _friendRequestService.SendRequestAsync(userId, vm.SelectedUserId);

            if (result)
            {
                TempData["SuccessMessage"] = "Solicitud de amistad enviada correctamente.";
            }
            else
            {
                TempData["ErrorMessage"] = "No se pudo enviar la solicitud de amistad.";
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Accept(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return RedirectToAction("Login", "User");

            var result = await _friendRequestService.AcceptRequestAsync(userId, id);

            if (result)
            {
                TempData["SuccessMessage"] = "Solicitud aceptada correctamente. Ahora son amigos.";
            }
            else
            {
                TempData["ErrorMessage"] = "No se pudo aceptar la solicitud.";
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return RedirectToAction("Login", "User");

            var result = await _friendRequestService.RejectRequestAsync(userId, id);

            if (result)
            {
                TempData["SuccessMessage"] = "Solicitud rechazada.";
            }
            else
            {
                TempData["ErrorMessage"] = "No se pudo rechazar la solicitud.";
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return RedirectToAction("Login", "User");

            var result = await _friendRequestService.CancelRequestAsync(userId, id);

            if (result)
            {
                TempData["SuccessMessage"] = "Solicitud cancelada correctamente.";
            }
            else
            {
                TempData["ErrorMessage"] = "No se pudo cancelar la solicitud.";
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Hide(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return RedirectToAction("Login", "User");

            var result = await _friendRequestService.HideRequestAsync(userId, id);

            if (result)
            {
                TempData["SuccessMessage"] = "Solicitud eliminada del historial correctamente.";
            }
            else
            {
                TempData["ErrorMessage"] = "No se pudo eliminar la solicitud del historial.";
            }

            return RedirectToAction("Index");
        }
    }
}
