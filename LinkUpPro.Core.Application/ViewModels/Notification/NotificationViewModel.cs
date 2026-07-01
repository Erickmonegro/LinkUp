using System;
using System.ComponentModel.DataAnnotations;

namespace LinkUpPro.Core.Application.ViewModels.Notification
{
    public class NotificationViewModel : BasicViewModel<int>
    {
        [Required(ErrorMessage = "El Id de usuario es requerido.")]
        public string UserId { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "El contenido es requerido.")]
        [Display(Name = "Mensaje")]
        public string Content { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "El tipo de notificación es requerido.")]
        public string NotificationType { get; set; } = string.Empty;
        
        public bool IsRead { get; set; }
        
        public int? RelatedEntityId { get; set; }
        
        [Display(Name = "Fecha")]
        public DateTime CreatedAt { get; set; }
    }
}
