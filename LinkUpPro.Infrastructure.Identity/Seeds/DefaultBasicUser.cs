using LinkUpPro.Infrastructure.Identity.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace LinkUpPro.Infrastructure.Identity.Seeds
{
    public static class DefaultBasicUser
    {
        public static async Task SeedAsync(UserManager<AppUser> userManager)
        {
            AppUser user = new()
            {
                UserName = "basicuser",
                Name = "Basic",
                LastName = "User",
                Email = "basicuser@gmail.com",
                EmailConfirmed = true,
                PhoneNumberConfirmed = true,
                ProfilePhoto = ""
            };

            if (await userManager.Users.AllAsync(u => u.Id != user.Id))
            {
                var foundUser = await userManager.FindByEmailAsync(user.Email);
                if (foundUser == null)
                {
                    await userManager.CreateAsync(user, "123Pa$$word!");
                    await userManager.AddToRoleAsync(user, "StandardUser");
                }
            }

            AppUser user2 = new()
            {
                UserName = "carlosm",
                Name = "Carlos",
                LastName = "Mendoza",
                Email = "carlos@gmail.com",
                EmailConfirmed = true,
                PhoneNumberConfirmed = true,
                ProfilePhoto = ""
            };
            if (await userManager.FindByEmailAsync(user2.Email) == null)
            {
                await userManager.CreateAsync(user2, "123Pa$$word!");
                await userManager.AddToRoleAsync(user2, "StandardUser");
            }

            AppUser user3 = new()
            {
                UserName = "anagomez",
                Name = "Ana",
                LastName = "Gómez",
                Email = "ana@gmail.com",
                EmailConfirmed = true,
                PhoneNumberConfirmed = true,
                ProfilePhoto = ""
            };
            if (await userManager.FindByEmailAsync(user3.Email) == null)
            {
                await userManager.CreateAsync(user3, "123Pa$$word!");
                await userManager.AddToRoleAsync(user3, "StandardUser");
            }
        }
    }
}
