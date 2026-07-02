using LinkUpPro.Core.Application.DTOs.NotificationDTO;
using LinkUpPro.Core.Application.Interfaces;
using LinkUpPro.Core.Domain.Common.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace LinkUpPro.Controllers
{
    [Authorize]
    public class ReactionController : Controller
    {
        private readonly IReactionService _reactionService;
        private readonly IPostService _postService;
        private readonly INotificationService _notificationService;
        private readonly IUserService _userService;

        public ReactionController(
            IReactionService reactionService,
            IPostService postService,
            INotificationService notificationService,
            IUserService userService)
        {
            _reactionService = reactionService;
            _postService = postService;
            _notificationService = notificationService;
            _userService = userService;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Toggle(int postId, bool isLike, string? returnUrl)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return RedirectToAction("Login", "Login");

            var post = await _postService.GetByIdAsync(postId);
            if (post == null)
            {
                return RedirectToAction("Index", "Home");
            }

           
            var existingReaction = post.Reactions?.FirstOrDefault(r => r.UserId == userId);
            var isNew = existingReaction == null;
            var isChanged = existingReaction != null && 
                            ((isLike && existingReaction.ReactionType == ReactionType.Dislike) ||
                             (!isLike && existingReaction.ReactionType == ReactionType.Like));

            
            await _reactionService.ToggleReactionAsync(postId, userId, isLike);

            if ((isNew || isChanged) && post.AuthorId != userId)
            {
                var currentUser = await _userService.GetUserById(userId);
                var triggerName = currentUser != null ? $"{currentUser.FirstName} {currentUser.LastName}" : "Un usuario";
                var reactionText = isLike ? "Me gusta" : "No me gusta";

                var notification = new NotificationDTO
                {
                    Id = 0,
                    ReceiverId = post.AuthorId,
                    TriggerUserId = userId,
                    TriggerUserName = triggerName,
                    TriggerUserPhotoUrl = currentUser?.PhotoUrl,
                    ActivityType = isChanged ? NotificationType.ChangeReactions : NotificationType.Reaction,
                    Description = $"{triggerName} reaccionó con {reactionText} a tu publicación.",
                    RelatedEntityId = postId,
                    Status = NotificationStatus.Unread
                };

                await _notificationService.AddAsync(notification);
            }

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Home");
        }
    }
}
