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

        public CommentController(
            ICommentService commentService,
            IPostService postService,
            INotificationService notificationService,
            IUserService userService)
        {
            _commentService = commentService;
            _postService = postService;
            _notificationService = notificationService;
            _userService = userService;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SaveCommentViewModel vm)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return RedirectToAction("Login", "User");

            if (string.IsNullOrWhiteSpace(vm.Content))
            {
                TempData["CommentError"] = "El comentario no puede estar vacío.";
                return RedirectToAction("Index", "Home");
            }

            if (vm.Content.Length > 500)
            {
                TempData["CommentError"] = "El comentario debe tener un máximo de 500 caracteres.";
                return RedirectToAction("Index", "Home");
            }

            var post = await _postService.GetByIdAsync(vm.PostId);
            if (post == null)
            {
                TempData["CommentError"] = "La publicación no existe o no está activa.";
                return RedirectToAction("Index", "Home");
            }

            if (post.CommentStatus == CommentStatus.NotAllowed)
            {
                TempData["CommentError"] = "Los comentarios están desactivados para esta publicación.";
                return RedirectToAction("Index", "Home");
            }

            var commentDto = new CommentDTO
            {
                Id = 0,
                Content = vm.Content.Trim(),
                PostId = vm.PostId,
                ParentCommentId = vm.ParentCommentId,
                AuthorId = userId,
                AuthorName = ""
            };

            CommentDTO savedComment;
            if (vm.ParentCommentId.HasValue)
            {
                savedComment = await _commentService.ReplyToCommentAsync(vm.ParentCommentId.Value, commentDto);

                // Notify parent comment author if not self
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

                // Notify post author if not self
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
            if (userId == null) return RedirectToAction("Login", "User");

            var comment = await _commentService.GetByIdAsync(id);
            if (comment == null || comment.AuthorId != userId)
            {
                TempData["CommentError"] = "No posee permisos para editar este contenido.";
                return RedirectToAction("Index", "Home");
            }

            if (string.IsNullOrWhiteSpace(content))
            {
                TempData["CommentError"] = "El comentario no puede estar vacío.";
                return RedirectToAction("Index", "Home");
            }

            if (content.Length > 500)
            {
                TempData["CommentError"] = "El comentario debe tener un máximo de 500 caracteres.";
                return RedirectToAction("Index", "Home");
            }

            comment.Content = content.Trim();
            // Wait, we need to map back to save. Yes, we call UpdateAsync.
            await _commentService.UpdateAsync(comment, id);

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return RedirectToAction("Login", "User");

            var comment = await _commentService.GetByIdAsync(id);
            if (comment == null || comment.AuthorId != userId)
            {
                TempData["CommentError"] = "No posee permisos para realizar esta acción.";
                return RedirectToAction("Index", "Home");
            }

            // Check if it has replies
            var hasReplies = (comment.Replies != null && comment.Replies.Any()) || await _commentService.HasRepliesAsync(id);
            if (hasReplies)
            {
                // Has replies: substitute content
                comment.Content = "comentario eliminado";
                await _commentService.UpdateAsync(comment, id);
            }
            else
            {
                // No replies: physical deletion
                await _commentService.DeleteAsync(id);
            }

            return RedirectToAction("Index", "Home");
        }
    }
}
