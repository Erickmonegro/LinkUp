using AutoMapper;
using LinkUpPro.Core.Application.DTOs.NotificationDTO;
using LinkUpPro.Core.Application.Interfaces;
using LinkUpPro.Core.Application.ViewModels.Notification;
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
    public class NotificationController : Controller
    {
        private readonly INotificationService _notificationService;
        private readonly IPostService _postService;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;

        public NotificationController(
            INotificationService notificationService,
            IPostService postService,
            IUserService userService,
            IMapper mapper)
        {
            _notificationService = notificationService;
            _postService = postService;
            _userService = userService;
            _mapper = mapper;
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return RedirectToAction("Login", "Login");

            var notifications = await _notificationService.GetUserNotificationsAsync(userId);

            var sortedNotifications = notifications
                .OrderByDescending(n => n.Created) 
                .ToList();

            var vms = _mapper.Map<List<NotificationViewModel>>(sortedNotifications);

          
            foreach (var vm in vms)
            {
                var originalDto = sortedNotifications.First(n => n.Id == vm.Id);
                ViewData[$"Photo-{vm.Id}"] = originalDto.TriggerUserPhotoUrl;
                ViewData[$"Name-{vm.Id}"] = originalDto.TriggerUserName;
            }

            return View(vms);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return RedirectToAction("Login", "Login");

            var notification = await _notificationService.GetByIdAsync(id);
            if (notification == null || notification.ReceiverId != userId)
            {
                return RedirectToAction("AccessDenied", "Login");
            }

            notification.Status = NotificationStatus.Read;
            await _notificationService.UpdateAsync(notification, id);

            if (notification.RelatedEntityId.HasValue)
            {
              
                if (notification.ActivityType == NotificationType.BattleChallenge ||
                    notification.ActivityType == NotificationType.BattleAccepted ||
                    notification.ActivityType == NotificationType.BattleRejected)
                {
                    return RedirectToAction("Details", "Battleship", new { id = notification.RelatedEntityId.Value });
                }

              
                var post = await _postService.GetByIdAsync(notification.RelatedEntityId.Value);
                if (post != null && post.IsActive)
                {
                    var anchor = $"#post-{post.Id}";

                    if (post.AuthorId == userId)
                    {
                      
                        var profileUrl = Url.Action("Index", "Profile") + anchor;
                        return Redirect(profileUrl!);
                    }
                    else
                    {
                      
                        var friendProfileUrl = Url.Action("Profile", "Friendship", new { id = post.AuthorId }) + anchor;
                        return Redirect(friendProfileUrl!);
                    }
                }
                else
                {
                    TempData["ErrorMessage"] = "El contenido relacionado con esta notificación ya no se encuentra disponible.";
                }
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return RedirectToAction("Login", "Login");

            await _notificationService.MarkAllAsReadAsync(userId);
            TempData["SuccessMessage"] = "Todas las notificaciones fueron marcadas como leídas.";
            return RedirectToAction("Index");
        }
    }
}
