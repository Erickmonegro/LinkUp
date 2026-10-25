using AutoMapper;
using LinkUpPro.Core.Application.Interfaces;
using LinkUpPro.Core.Application.ViewModels.FriendRequest;
using LinkUpPro.Core.Application.ViewModels.Friendship;
using LinkUpPro.Core.Domain.Common.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
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
        private readonly IMapper _mapper;

        public FriendRequestController(
            IFriendRequestService friendRequestService,
            IUserService userService,
            IFriendshipService friendshipService,
            IMapper mapper)
        {
            _friendRequestService = friendRequestService;
            _userService = userService;
            _friendshipService = friendshipService;
            _mapper = mapper;
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return RedirectToAction("Login", "Login");

            var pendingReceived = await _friendRequestService.GetPendingReceivedAsync(userId);
            var sentRequests = await _friendRequestService.GetSentRequestsAsync(userId);
            var pendingCount = await _friendRequestService.GetPendingCountAsync(userId);

            var receivedVMs = _mapper.Map<List<FriendRequestViewModel>>(pendingReceived);
            receivedVMs.ForEach(r => r.IsIncoming = true);

            var sentVMs = _mapper.Map<List<FriendRequestViewModel>>(sentRequests);
            sentVMs.ForEach(r => r.IsIncoming = false);

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
            if (userId == null) return RedirectToAction("Login", "Login");

            var currentFriends = await _friendshipService.GetFriendsAsync(userId);
            var friendIds = currentFriends.Select(f => f.FriendId).ToHashSet();

            var searchQuery = searchTerm ?? "";
            var availableUsers = await _friendRequestService.SearchAvailableUsersAsync(userId, searchQuery);

            var availableFriendVMs = _mapper.Map<List<FriendViewModel>>(availableUsers);
            availableFriendVMs.ForEach(u => u.IsAlreadyFriend = friendIds.Contains(u.UserId));

            var currentFriendVMs = _mapper.Map<List<FriendViewModel>>(currentFriends);

            var vm = new SendRequestViewModel
            {
                SearchTerm = searchQuery,
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
            if (userId == null) return RedirectToAction("Login", "Login");

            if (string.IsNullOrEmpty(vm.SelectedUserId))
            {
                TempData["ErrorMessage"] = "Debe seleccionar un usuario para enviar la solicitud de amistad.";
                return RedirectToAction("New", new { searchTerm = vm.SearchTerm });
            }

            var targetUser = await _userService.GetUserById(vm.SelectedUserId);
            if (targetUser == null || !targetUser.IsActive)
            {
                TempData["ErrorMessage"] = "No se puede enviar la solicitud porque uno de los usuarios se encuentra inactivo.";
                return RedirectToAction("New", new { searchTerm = vm.SearchTerm });
            }

            var friends = await _friendshipService.GetFriendsAsync(userId);
            if (friends.Any(f => f.FriendId == vm.SelectedUserId))
            {
                TempData["ErrorMessage"] = "Este usuario ya forma parte de su lista de amigos.";
                return RedirectToAction("New", new { searchTerm = vm.SearchTerm });
            }

            var sentPending = await _friendRequestService.GetSentRequestsAsync(userId);
            if (sentPending.Any(r => r.ReceiverId == vm.SelectedUserId && r.Status == FriendRequestStatus.Pending))
            {
                TempData["ErrorMessage"] = "Ya envió una solicitud de amistad a este usuario y se encuentra pendiente de respuesta.";
                return RedirectToAction("New", new { searchTerm = vm.SearchTerm });
            }

            var receivedPending = await _friendRequestService.GetPendingReceivedAsync(userId);
            if (receivedPending.Any(r => r.SenderId == vm.SelectedUserId))
            {
                TempData["ErrorMessage"] = "Este usuario ya le envió una solicitud de amistad. Debe aceptarla o rechazarla desde sus solicitudes pendientes.";
                return RedirectToAction("Index");
            }

            var result = await _friendRequestService.SendRequestAsync(userId, vm.SelectedUserId);

            if (result)
            {
                TempData["SuccessMessage"] = "La solicitud de amistad fue enviada correctamente.";
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
            if (userId == null) return RedirectToAction("Login", "Login");

            var result = await _friendRequestService.AcceptRequestAsync(userId, id);

            if (result)
            {
                TempData["SuccessMessage"] = "La solicitud fue aceptada correctamente.";
            }
            else
            {
                TempData["ErrorMessage"] = "Esta solicitud ya no se encuentra disponible para ser aceptada.";
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return RedirectToAction("Login", "Login");

            var result = await _friendRequestService.RejectRequestAsync(userId, id);

            if (result)
            {
                TempData["SuccessMessage"] = "La solicitud fue rechazada correctamente.";
            }
            else
            {
                TempData["ErrorMessage"] = "No posee permisos para realizar esta acción sobre la solicitud.";
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return RedirectToAction("Login", "Login");

            var result = await _friendRequestService.CancelRequestAsync(userId, id);

            if (result)
            {
                TempData["SuccessMessage"] = "La solicitud fue cancelada correctamente.";
            }
            else
            {
                TempData["ErrorMessage"] = "No posee permisos para realizar esta acción sobre la solicitud.";
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Hide(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return RedirectToAction("Login", "Login");

            var result = await _friendRequestService.HideRequestAsync(userId, id);

            if (result)
            {
                TempData["SuccessMessage"] = "La solicitud fue eliminada de su historial.";
            }
            else
            {
                TempData["ErrorMessage"] = "No posee permisos para realizar esta acción sobre la solicitud.";
            }

            return RedirectToAction("Index");
        }
    }
}
