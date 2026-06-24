using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkUpPro.Infrastructure.Identity.Entities
{
    public class AppUser
    {
        public required string Name { get; set; }
        public required string LastName { get; set; }
        public required string ProfilePhoto { get; set; }
    }
}
