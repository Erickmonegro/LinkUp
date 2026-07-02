using AutoMapper;
using LinkUpPro.Core.Application.DTOs.CommentDTO;
using LinkUpPro.Core.Application.DTOs.NotificationDTO;
using LinkUpPro.Core.Application.Interfaces;
using LinkUpPro.Core.Application.ViewModels.Comment;
using LinkUpPro.Core.Domain.Common.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
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
        private readonly IMapper _mapper;

        public CommentController(
            ICommentService commentService,
            IPostService postService,
            INotificationService notificationService,
            IUserService userService,
            IMapper mapper)
        {
            _commentService = commentService;
            _postService = postService;
            _notificationService = notificationService;
            _userService = userService;
            _mapper = mapper;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SaveCommentViewModel vm)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return RedirectToAction("Login", "Login");

            if (!ModelState.IsValid)
            {
                return RedirectToAction("Index", "Home");
            }

            var post = await _postService.GetByIdAsync(vm.PostId);
            if (post == null || post.CommentStatus == CommentStatus.NotAllowed)
            {
                return RedirectToAction("Index", "Home");
            }

            var commentDto = _mapper.Map<CommentDTO>(vm);
            commentDto.Id = 0;
            commentDto.Content = vm.Content.Trim();
            commentDto.AuthorId = userId;
            commentDto.AuthorName = "";

            CommentDTO savedComment;
            if (vm.ParentCommentId.HasValue)
            {
                savedComment = await _commentService.ReplyToCommentAsync(vm.ParentCommentId.Value, commentDto);

                var parentComment = await _commentService.GetByIdAsync(vm.ParentCommentId.Value);
                if (parentComment != null && parentComment.AuthorId != userId)
                {
                    var currentUser = await _userService.GetUserById(userId);
                    var triggerName = currentUser != null ? $"{currentUser.FirstName} {currentUser.LastName}" : "Un usuario";

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
                    var triggerName = currentUser != null ? $"{currentUser.FirstName} {currentUser.LastName}" : "Un usuario";

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

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, string content)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return RedirectToAction("Login", "Login");

            var comment = await _commentService.GetByIdAsync(id);
            if (comment == null || comment.AuthorId != userId || string.IsNullOrWhiteSpace(content) || content.Length > 500)
            {
                return RedirectToAction("Index", "Home");
            }

            comment.Content = content.Trim();
            await _commentService.UpdateAsync(comment, id);

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return RedirectToAction("Login", "Login");

            var comment = await _commentService.GetByIdAsync(id);
            if (comment == null || comment.AuthorId != userId)
            {
                return RedirectToAction("Index", "Home");
            }

            var hasReplies = (comment.Replies != null && comment.Replies.Any()) || await _commentService.HasRepliesAsync(id);
            if (hasReplies)
            {
                comment.Content = "comentario eliminado";
                await _commentService.UpdateAsync(comment, id);
            }
            else
            {
                await _commentService.DeleteAsync(id);
            }

            return RedirectToAction("Index", "Home");
        }
    }
}
