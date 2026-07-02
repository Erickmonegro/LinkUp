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

        public FriendshipController(
            IFriendshipService friendshipService,
            IUserService userService,
            IPostService postService)
        {
            _friendshipService = friendshipService;
            _userService = userService;
            _postService = postService;
        }

        public async Task<IActionResult> Index(
            string? textFilter,
            string? friendIdFilter,
            int? contentTypeFilter,
            DateTime? dateFrom,
            DateTime? dateTo,
            int? editionStatusFilter)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return RedirectToAction("Login", "User");

            if (dateFrom.HasValue && dateTo.HasValue && dateFrom.Value > dateTo.Value)
            {
                ModelState.AddModelError("", "La fecha inicial no puede ser posterior a la fecha final.");
                TempData["ErrorMessage"] = "La fecha inicial (Desde) no puede ser posterior a la fecha final (Hasta).";
            }

            var friends = await _friendshipService.GetFriendsAsync(userId);
            var friendsCount = await _friendshipService.GetActiveFriendsCountAsync(userId);
            var postsCount = await _friendshipService.GetFriendPostsCountAsync(userId);

            // Filter posts if model state is valid
            var posts = new List<Core.Application.DTOs.PostDTO.PostDTO>();
            if (ModelState.IsValid)
            {
                posts = await _friendshipService.GetFriendPostsAsync(
                    userId,
                    textFilter,
                    friendIdFilter,
                    contentTypeFilter,
                    dateFrom,
                    dateTo,
                    editionStatusFilter
                );
            }

            var friendViewModels = friends.Select(f => new FriendViewModel
            {
                Id = f.Id.ToString(), // Fixed: converted int to string to match property type
                UserId = f.FriendId,
                FullName = f.FriendName,
                ProfilePhoto = f.FriendPhotoUrl,
                UserName = f.FriendId,
                MutualFriendsCount = f.MutualFriendsCount
            }).ToList();

            // Populate UserNames for friend list
            foreach (var fvm in friendViewModels)
            {
                var u = await _userService.GetUserById(fvm.UserId);
                if (u != null) fvm.UserName = u.UserName;
            }

            var vm = new FriendListViewModel
            {
                ActiveFriendsCount = friendsCount,
                FriendPostsCount = postsCount,
                Friends = friendViewModels,
                Posts = posts
            };

            // Filters repopulation
            ViewBag.TextFilter = textFilter;
            ViewBag.FriendIdFilter = friendIdFilter;
            ViewBag.ContentTypeFilter = contentTypeFilter ?? (int)ContentType.All;
            ViewBag.DateFrom = dateFrom?.ToString("yyyy-MM-dd");
            ViewBag.DateTo = dateTo?.ToString("yyyy-MM-dd");
            ViewBag.EditionStatusFilter = editionStatusFilter ?? (int)EditionStatus.All;

            return View(vm);
        }

        public async Task<IActionResult> Profile(string id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return RedirectToAction("Login", "User");

            var friend = await _friendshipService.GetFriendProfileAsync(userId, id);
            if (friend == null)
            {
                return RedirectToAction("AccessDenied", "User");
            }

            var friends = await _friendshipService.GetFriendsAsync(userId);
            var friendship = friends.FirstOrDefault(f => f.FriendId == id);
            var friendshipId = friendship?.Id ?? 0;

            // Fetch the friend's posts with privacy SoloAmigos (FriendsOnly)
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
            var mutualFriendsVMs = mutualUsersDto.Select(u => new FriendViewModel
            {
                Id = "",
                UserId = u.Id,
                FullName = $"{u.FirstName} {u.LastName}",
                ProfilePhoto = u.PhotoUrl,
                UserName = u.UserName
            }).ToList();

            var mutualCount = mutualFriendsVMs.Count;

            ViewBag.FriendshipId = friendshipId;
            ViewBag.Posts = visiblePosts;

            var vm = new FriendProfileViewModel
            {
                UserId = friend.Id,
                FullName = $"{friend.FirstName} {friend.LastName}",
                ProfilePhoto = friend.PhotoUrl,
                CoverPhoto = friend.CoverPhotoUrl,
                UserName = friend.UserName,
                Phone = friend.PhoneNumber,
                Email = friend.Email,
                TotalFriends = await _friendshipService.GetActiveFriendsCountAsync(id),
                MutualFriendsCount = mutualCount,
                IsFriend = true,
                MutualFriends = mutualFriendsVMs
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int friendshipId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return RedirectToAction("Login", "User");

            var result = await _friendshipService.DeleteFriendshipAsync(userId, friendshipId);

            if (result)
            {
                TempData["SuccessMessage"] = "La amistad fue eliminada correctamente.";
            }
            else
            {
                TempData["ErrorMessage"] = "No se pudo eliminar la amistad.";
            }

            return RedirectToAction("Index");
        }
    }
}
