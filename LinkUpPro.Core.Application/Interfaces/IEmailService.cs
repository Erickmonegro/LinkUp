using LinkUpPro.Core.Application.DTOs.Email;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkUpPro.Core.Application.Interfaces
{
    public interface IEmailService
    {
        Task SendAsync(EmailRequestDTO emailRequestDto);
    }
}
