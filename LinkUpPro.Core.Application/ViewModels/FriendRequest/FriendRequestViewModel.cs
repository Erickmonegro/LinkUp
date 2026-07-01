using System;
using System.ComponentModel.DataAnnotations;

namespace LinkUpPro.Core.Application.ViewModels.FriendRequest
{
    public class FriendRequestViewModel : BasicViewModel<int>
    {
        [Required(ErrorMessage = "El Id del remitente es requerido.")]
        public string SenderId { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre del remitente es requerido.")]
        [Display(Name = "Enviado por")]
        public string SenderName { get; set; } = string.Empty;

        public string? SenderProfilePhoto { get; set; }

        [Required(ErrorMessage = "El Id del destinatario es requerido.")]
        public string ReceiverId { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre del destinatario es requerido.")]
        [Display(Name = "Recibido por")]
        public string ReceiverName { get; set; } = string.Empty;

        public string? ReceiverProfilePhoto { get; set; }

        [Required(ErrorMessage = "El estado de la solicitud es requerido.")]
        [Display(Name = "Estado")]
        public string Status { get; set; } = string.Empty;

        [Display(Name = "Fecha de Creación")]
        public DateTime CreatedAt { get; set; }

        [Display(Name = "Fecha de Respuesta")]
        public DateTime? RespondedAt { get; set; }

        public bool IsHidden { get; set; }

        public bool IsIncoming { get; set; }
    }
}
