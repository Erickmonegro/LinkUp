using AutoMapper;
using LinkUpPro.Core.Application.Interfaces;
using LinkUpPro.Core.Application.ViewModels.Friendship;
using LinkUpPro.Core.Domain.Common.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace LinkUpPro.Controllers
{
    [Authorize]
    public class FriendshipController : Controller
    {
        private readonly IFriendshipService _friendshipService;
        private readonly IUserService _userService;
        private readonly IPostService _postService;
        private readonly IMapper _mapper;

        public FriendshipController(
            IFriendshipService friendshipService,
            IUserService userService,
            IPostService postService,
            IMapper mapper)
        {
            _friendshipService = friendshipService;
            _userService = userService;
            _postService = postService;
            _mapper = mapper;
        }

        public async Task<IActionResult> Index(
            string? textFilter,
            string? friendIdFilter,
            int? contentTypeFilter,
            DateTime? dateFrom,
            DateTime? dateTo,
            int? editionStatusFilter,
            string? friendSearch)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return RedirectToAction("Login", "Login");

            var friends = await _friendshipService.GetFriendsAsync(userId);
            var friendsCount = await _friendshipService.GetActiveFriendsCountAsync(userId);
            var postsCount = await _friendshipService.GetFriendPostsCountAsync(userId);

            var posts = await _friendshipService.GetFriendPostsAsync(
                userId,
                textFilter,
                friendIdFilter,
                contentTypeFilter,
                dateFrom,
                dateTo,
                editionStatusFilter
            );

            var friendViewModels = _mapper.Map<List<FriendViewModel>>(friends);

            foreach (var fvm in friendViewModels)
            {
                var u = await _userService.GetUserById(fvm.UserId);
                if (u != null) fvm.UserName = u.UserName;
            }

            if (!string.IsNullOrWhiteSpace(friendSearch))
            {
                var cleanSearch = friendSearch.Trim().ToLower();
                friendViewModels = friendViewModels
                    .Where(f => (f.FullName != null && f.FullName.ToLower().Contains(cleanSearch)) || 
                                (f.UserName != null && f.UserName.ToLower().Contains(cleanSearch)))
                    .ToList();
            }

            var vm = new FriendListViewModel
            {
                ActiveFriendsCount = friendsCount,
                FriendPostsCount = postsCount,
                Friends = friendViewModels,
                Posts = posts
            };

            ViewBag.TextFilter = textFilter;
            ViewBag.FriendIdFilter = friendIdFilter;
            ViewBag.ContentTypeFilter = contentTypeFilter ?? (int)ContentType.All;
            ViewBag.DateFrom = dateFrom?.ToString("yyyy-MM-dd");
            ViewBag.DateTo = dateTo?.ToString("yyyy-MM-dd");
            ViewBag.EditionStatusFilter = editionStatusFilter ?? (int)EditionStatus.All;
            ViewBag.FriendSearch = friendSearch;

            return View(vm);
        }

        public async Task<IActionResult> Profile(string id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return RedirectToAction("Login", "Login");

            var friend = await _friendshipService.GetFriendProfileAsync(userId, id);
            if (friend == null)
            {
                TempData["ErrorMessage"] = "No posee permisos para visualizar el perfil de este usuario.";
                return RedirectToAction(nameof(Index));
            }

            var friends = await _friendshipService.GetFriendsAsync(userId);
            var friendship = friends.FirstOrDefault(f => f.FriendId == id);
            var friendshipId = friendship?.Id ?? 0;

            var allFriendPosts = await _postService.GetUserPostsFilteredAsync(
                id,
                null,
                null,
                null,
                null,
                null
            );

            var visiblePosts = allFriendPosts
                .Where(p => p.PrivacyMode != PrivacyMode.OnlyMe)
                .ToList();

            var mutualUsersDto = await _friendshipService.GetMutualFriendsAsync(userId, id);
            var mutualFriendsVMs = _mapper.Map<List<FriendViewModel>>(mutualUsersDto);

            var mutualCount = mutualFriendsVMs.Count;

            ViewBag.FriendshipId = friendshipId;
            ViewBag.Posts = visiblePosts;

            var vm = _mapper.Map<FriendProfileViewModel>(friend);
            vm.TotalFriends = await _friendshipService.GetActiveFriendsCountAsync(id);
            vm.MutualFriendsCount = mutualCount;
            vm.IsFriend = true;
            vm.MutualFriends = mutualFriendsVMs;

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int friendshipId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return RedirectToAction("Login", "Login");

            var result = await _friendshipService.DeleteFriendshipAsync(userId, friendshipId);

            if (result)
            {
                TempData["SuccessMessage"] = "La amistad fue eliminada correctamente.";
            }
            else
            {
                TempData["ErrorMessage"] = "La amistad seleccionada ya no se encuentra disponible.";
            }

            return RedirectToAction("Index");
        }
    }
}
