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
                var currentFriends = await _friendshipService.GetFriendsAsync(userId);
                var friendIds = currentFriends.Select(f => f.FriendId).ToHashSet();
                var availableUsers = await _friendRequestService.SearchAvailableUsersAsync(userId, vm.SearchTerm ?? "");
                var availableFriendVMs = _mapper.Map<List<FriendViewModel>>(availableUsers);
                availableFriendVMs.ForEach(u => u.IsAlreadyFriend = friendIds.Contains(u.UserId));
                var currentFriendVMs = _mapper.Map<List<FriendViewModel>>(currentFriends);

                vm.AvailableUsers = availableFriendVMs;
                vm.CurrentFriends = currentFriendVMs;
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
            if (userId == null) return RedirectToAction("Login", "Login");

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
            if (userId == null) return RedirectToAction("Login", "Login");

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
            if (userId == null) return RedirectToAction("Login", "Login");

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
            if (userId == null) return RedirectToAction("Login", "Login");

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
