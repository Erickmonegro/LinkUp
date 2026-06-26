using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkUpPro.Core.Application.DTOs
{
    public class BasicDTO<TKey>
    {
        
        public required TKey Id { get; set; }

        public DateTime Created { get; set; } = DateTime.UtcNow;

        public DateTime Updated { get; set; } = DateTime.UtcNow;
    }
}
