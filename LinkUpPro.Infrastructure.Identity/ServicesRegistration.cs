using LinkUpPro.Core.Application.Interfaces;
using LinkUpPro.Infrastructure.Identity.Contexts;
using LinkUpPro.Infrastructure.Identity.Entities;
using LinkUpPro.Infrastructure.Identity.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;

namespace LinkUpPro.Infrastructure.Identity
{
    public class EmailConfirmationTokenProvider<TUser> : DataProtectorTokenProvider<TUser> where TUser : class
    {
        public EmailConfirmationTokenProvider(
            IDataProtectionProvider dataProtectionProvider,
            IOptions<EmailConfirmationTokenProviderOptions> options,
            ILogger<DataProtectorTokenProvider<TUser>> logger)
            : base(dataProtectionProvider, options, logger)
        {
        }
    }

    public class EmailConfirmationTokenProviderOptions : DataProtectionTokenProviderOptions
    {
        public EmailConfirmationTokenProviderOptions()
        {
            Name = "EmailConfirmationTokenProvider";
            TokenLifespan = TimeSpan.FromHours(24);
        }
    }

    public static class ServicesRegistration
    {
        public static void AddIdentityLayerIocForWebApp(this IServiceCollection services, IConfiguration config)
        {
            GeneralConfiguration(services, config);

            #region Identity
            services.AddIdentity<AppUser, IdentityRole>(options =>
            {
                options.SignIn.RequireConfirmedAccount = false;
                options.SignIn.RequireConfirmedEmail = true;
                options.User.RequireUniqueEmail = true;

                // Lockout: 5 intentos fallidos = bloqueo 15 minutos
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
                options.Lockout.AllowedForNewUsers = true;

                // Reglas de contraseña estrictas según pliego (mayúscula, minúscula, número, especial, >=8)
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequiredLength = 8;

                // Token de activación con vigencia de 24 horas según pliego
                options.Tokens.EmailConfirmationTokenProvider = "EmailConfirmation";
            })
            .AddEntityFrameworkStores<IdentityUserContext>()
            .AddDefaultTokenProviders()
            .AddTokenProvider<EmailConfirmationTokenProvider<AppUser>>("EmailConfirmation");

            // Configurar vigencia del token de activación a 24 horas
            services.Configure<EmailConfirmationTokenProviderOptions>(options =>
            {
                options.TokenLifespan = TimeSpan.FromHours(24);
            });

            // Configurar vigencia de tokens de recuperación / restablecimiento (1 hora según pliego)
            services.Configure<DataProtectionTokenProviderOptions>(options =>
            {
                options.TokenLifespan = TimeSpan.FromHours(1);
            });

            services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Login/Login";
                options.AccessDeniedPath = "/Login/AccessDenied";
                options.LogoutPath = "/Login/Logout";

                // Cookie security per requirements:
                // HttpOnly impide lectura desde JavaScript.
                options.Cookie.HttpOnly = true;
                options.Cookie.SecurePolicy = Microsoft.AspNetCore.Http.CookieSecurePolicy.SameAsRequest;
                options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax;

                options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
                options.SlidingExpiration = true;

                options.Events = new Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationEvents
                {
                    OnSigningIn = async context =>
                    {
                        var identity = context.Principal?.Identity as System.Security.Claims.ClaimsIdentity;
                        if (context.Properties.IsPersistent)
                        {
                            context.Properties.ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7);
                            context.Properties.AllowRefresh = false;
                            context.CookieOptions.Expires = DateTimeOffset.UtcNow.AddDays(7);
                            context.CookieOptions.MaxAge = TimeSpan.FromDays(7);
                            identity?.AddClaim(new System.Security.Claims.Claim("IsPersistent", "true"));
                        }
                        else
                        {
                            context.Properties.ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30);
                            context.Properties.AllowRefresh = true;
                            context.Properties.IsPersistent = false;
                            context.CookieOptions.Expires = null;
                            context.CookieOptions.MaxAge = null;
                            identity?.AddClaim(new System.Security.Claims.Claim("IsPersistent", "false"));
                        }
                        await Task.CompletedTask;
                    },
                    OnValidatePrincipal = async context =>
                    {
                        if (!context.Properties.IsPersistent)
                        {
                            var currentUtc = DateTimeOffset.UtcNow;
                            var expiresUtc = context.Properties.ExpiresUtc ?? (context.Properties.IssuedUtc?.AddMinutes(30) ?? currentUtc.AddMinutes(30));
                            if (currentUtc >= expiresUtc)
                            {
                                context.RejectPrincipal();
                                await context.HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);
                                context.Response.Redirect("/Login/Login?timeout=true");
                            }
                        }
                        await Task.CompletedTask;
                    }
                };
            });

            #endregion

            #region Services
            services.AddScoped<IUserService, UserService>();
            #endregion
        }

        public static async Task RunIdentitySeedAsync(this IServiceProvider service)
        {
            using var scope = service.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            await Seeds.DefaultRoles.SeedAsync(roleManager);
            await Seeds.DefaultBasicUser.SeedAsync(userManager);
        }

        private static void GeneralConfiguration(IServiceCollection services, IConfiguration config)
        {
            #region Contexts
            if (config.GetValue<bool>("UseInMemoryDatabase"))
            {
                services.AddDbContext<IdentityUserContext>(options =>
                    options.UseInMemoryDatabase("IdentityAppDb"));
            }
            else
            {
                services.AddDbContext<IdentityUserContext>(options =>
                    options.UseSqlServer(config.GetConnectionString("DefaultConnection"),
                        b => b.MigrationsAssembly(typeof(IdentityUserContext).Assembly.FullName)));
            }
            #endregion
        }
    }
}
