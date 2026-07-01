using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

namespace LinkUpPro.Infrastructure.Identity.Seeds
{
    public static class DefaultRoles
    {
        public static async Task SeedAsync(RoleManager<IdentityRole> roleManager)
        {
            if (!await roleManager.RoleExistsAsync("StandardUser"))
            {
                await roleManager.CreateAsync(new IdentityRole("StandardUser"));
            }
            if (!await roleManager.RoleExistsAsync("Admin"))
            {
                await roleManager.CreateAsync(new IdentityRole("Admin"));
            }
        }
    }
}
