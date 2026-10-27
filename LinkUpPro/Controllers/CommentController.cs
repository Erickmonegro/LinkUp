using AutoMapper;
using LinkUpPro.Core.Application.DTOs.CommentDTO;
using LinkUpPro.Core.Application.DTOs.NotificationDTO;
using LinkUpPro.Core.Application.Interfaces;
using LinkUpPro.Core.Application.ViewModels.Comment;
using LinkUpPro.Core.Domain.Common.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace LinkUpPro.Controllers
{
    [Authorize]
    public class CommentController : Controller
    {
        private readonly ICommentService _commentService;
        private readonly IPostService _postService;
        private readonly INotificationService _notificationService;
        private readonly IUserService _userService;
        private readonly IFriendshipService _friendshipService;
        private readonly IMapper _mapper;

        public CommentController(
            ICommentService commentService,
            IPostService postService,
            INotificationService notificationService,
            IUserService userService,
            IFriendshipService friendshipService,
            IMapper mapper)
        {
            _commentService = commentService;
            _postService = postService;
            _notificationService = notificationService;
            _userService = userService;
            _friendshipService = friendshipService;
            _mapper = mapper;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SaveCommentViewModel vm)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return RedirectToAction("Login", "Login");

            if (!ModelState.IsValid || string.IsNullOrWhiteSpace(vm.Content))
            {
                return RedirectToAction("Index", "Home");
            }

            var post = await _postService.GetByIdAsync(vm.PostId);
            if (post == null || !post.IsActive || post.CommentStatus == CommentStatus.NotAllowed)
            {
                return RedirectToAction("Index", "Home");
            }

            // Validar privacidad de la publicación y amistad
            if (post.AuthorId != userId)
            {
                if (post.PrivacyMode == PrivacyMode.OnlyMe)
                {
                    return RedirectToAction("Index", "Home");
                }

                if (post.PrivacyMode == PrivacyMode.FriendsOnly)
                {
                    var areFriends = await _friendshipService.AreFriendsAsync(userId, post.AuthorId);
                    if (!areFriends)
                    {
                        return RedirectToAction("Index", "Home");
                    }
                }
            }

            var commentDto = _mapper.Map<CommentDTO>(vm);
            commentDto.Id = 0;
            commentDto.Content = vm.Content.Trim();
            commentDto.AuthorId = userId;
            commentDto.AuthorName = "";

            CommentDTO? savedComment;
            if (vm.ParentCommentId.HasValue)
            {
                savedComment = await _commentService.ReplyToCommentAsync(vm.ParentCommentId.Value, commentDto);

                var parentComment = await _commentService.GetByIdAsync(vm.ParentCommentId.Value);
                if (parentComment != null && parentComment.AuthorId != userId)
                {
                    var currentUser = await _userService.GetUserById(userId);
                    var triggerName = currentUser?.UserName ?? "Un usuario";

                    var notification = new NotificationDTO
                    {
                        Id = 0,
                        ReceiverId = parentComment.AuthorId,
                        TriggerUserId = userId,
                        TriggerUserName = triggerName,
                        TriggerUserPhotoUrl = currentUser?.PhotoUrl,
                        ActivityType = NotificationType.Reply,
                        Description = $"{triggerName} respondió tu comentario.",
                        RelatedEntityId = vm.PostId,
                        Status = NotificationStatus.Unread
                    };
                    await _notificationService.AddAsync(notification);
                }
            }
            else
            {
                savedComment = await _commentService.AddAsync(commentDto);

                if (post.AuthorId != userId)
                {
                    var currentUser = await _userService.GetUserById(userId);
                    var triggerName = currentUser?.UserName ?? "Un usuario";

                    var notification = new NotificationDTO
                    {
                        Id = 0,
                        ReceiverId = post.AuthorId,
                        TriggerUserId = userId,
                        TriggerUserName = triggerName,
                        TriggerUserPhotoUrl = currentUser?.PhotoUrl,
                        ActivityType = NotificationType.Comment,
                        Description = $"{triggerName} comentó tu publicación.",
                        RelatedEntityId = vm.PostId,
                        Status = NotificationStatus.Unread
                    };
                    await _notificationService.AddAsync(notification);
                }
            }

            var redirectUrl = Request.Headers["Referer"].ToString();
            if (!string.IsNullOrEmpty(redirectUrl))
            {
                var fragmentIdx = redirectUrl.IndexOf('#');
                if (fragmentIdx >= 0)
                {
                    redirectUrl = redirectUrl.Substring(0, fragmentIdx);
                }
                redirectUrl += $"#post-{vm.PostId}";
                return Redirect(redirectUrl);
            }

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, string content)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return RedirectToAction("Login", "Login");

            var comment = await _commentService.GetByIdAsync(id);
            if (comment == null)
            {
                return RedirectToAction("Index", "Home");
            }

            if (comment.AuthorId != userId)
            {
                TempData["ErrorMessage"] = "No posee permisos para editar este contenido.";
                return RedirectToAction("Index", "Home");
            }

            if (string.IsNullOrWhiteSpace(content) || content.Length > 500)
            {
                return RedirectToAction("Index", "Home");
            }

            comment.Content = content.Trim();
            comment.Updated = DateTime.UtcNow;
            comment.IsEdited = true;
            await _commentService.UpdateAsync(comment, id);

            var redirectUrl = Request.Headers["Referer"].ToString();
            if (!string.IsNullOrEmpty(redirectUrl))
            {
                var fragmentIdx = redirectUrl.IndexOf('#');
                if (fragmentIdx >= 0)
                {
                    redirectUrl = redirectUrl.Substring(0, fragmentIdx);
                }
                redirectUrl += $"#post-{comment.PostId}";
                return Redirect(redirectUrl);
            }

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return RedirectToAction("Login", "Login");

            var comment = await _commentService.GetByIdAsync(id);
            if (comment == null)
            {
                return RedirectToAction("Index", "Home");
            }

            if (comment.AuthorId != userId)
            {
                TempData["ErrorMessage"] = "No posee permisos para eliminar este contenido.";
                return RedirectToAction("Index", "Home");
            }

            var postId = comment.PostId;
            var hasReplies = (comment.Replies != null && comment.Replies.Any()) || await _commentService.HasRepliesAsync(id);
            if (hasReplies)
            {
                // Reemplazo textual exacto según pliego funcional (Pág. 27)
                comment.Content = "Este comentario fue eliminado.";
                comment.Updated = DateTime.UtcNow;
                await _commentService.UpdateAsync(comment, id);
            }
            else
            {
                await _commentService.DeleteAsync(id);
            }

            var redirectUrl = Request.Headers["Referer"].ToString();
            if (!string.IsNullOrEmpty(redirectUrl))
            {
                var fragmentIdx = redirectUrl.IndexOf('#');
                if (fragmentIdx >= 0)
                {
                    redirectUrl = redirectUrl.Substring(0, fragmentIdx);
                }
                redirectUrl += $"#post-{postId}";
                return Redirect(redirectUrl);
            }

            return RedirectToAction("Index", "Home");
        }
    }
}
