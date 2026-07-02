using LinkUpPro.Core.Application.Interfaces;
using LinkUpPro.Infrastructure.Identity.Contexts;
using LinkUpPro.Infrastructure.Identity.Entities;
using LinkUpPro.Infrastructure.Identity.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LinkUpPro.Infrastructure.Identity
{
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

                // Password complexity per requirements:
                // Mínimo 8 caracteres, al menos 1 mayúscula, al menos 1 carácter no alfanumérico.
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequiredLength = 8;
            })
            .AddEntityFrameworkStores<IdentityUserContext>()
            .AddDefaultTokenProviders();

            services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/User/Login";
                options.AccessDeniedPath = "/User/AccessDenied";

                // Cookie security per requirements:
                // HttpOnly impide lectura desde JavaScript.
                // SecurePolicy en SameAsRequest/Always asegura envío únicamente mediante HTTPS en ambientes publicados.
                // SameSite en Lax compatible con el flujo de autenticación.
                options.Cookie.HttpOnly = true;
                options.Cookie.SecurePolicy = Microsoft.AspNetCore.Http.CookieSecurePolicy.SameAsRequest;
                options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax;

                // Session timeout default
                options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
                options.SlidingExpiration = true;

                options.Events = new Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationEvents
                {
                    OnSigningIn = async context =>
                    {
                        var identity = context.Principal?.Identity as System.Security.Claims.ClaimsIdentity;
                        if (context.Properties.IsPersistent)
                        {
                            // Si selecciona "Mantener sesión iniciada":
                            // - Duración máxima de 7 días.
                            // - Conservada después de cerrar el navegador.
                            // - Al cumplirse los 7 días, deberá iniciar sesión nuevamente (AllowRefresh = false para no extenderla infinitamente).
                            context.Properties.ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7);
                            context.Properties.AllowRefresh = false;
                            context.CookieOptions.Expires = DateTimeOffset.UtcNow.AddDays(7);
                            context.CookieOptions.MaxAge = TimeSpan.FromDays(7);
                            identity?.AddClaim(new System.Security.Claims.Claim("IsPersistent", "true"));
                        }
                        else
                        {
                            // Si no selecciona la opción:
                            // - Duración de 30 minutos de inactividad (SlidingExpiration extiende con actividad).
                            // - Se cierra al cerrar el navegador (IsPersistent = false, cookie de sesión sin fecha de expiración en navegador).
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
                            var issuedUtc = context.Properties.IssuedUtc ?? DateTimeOffset.UtcNow;
                            if (DateTimeOffset.UtcNow.Subtract(issuedUtc) > TimeSpan.FromMinutes(30))
                            {
                                context.RejectPrincipal();
                                await context.HttpContext.SignOutAsync(Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme);
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

        #region Private Methods
        private static void GeneralConfiguration(IServiceCollection services, IConfiguration config)
        {
            #region Contexts
            if (config.GetValue<bool>("UseInMemoryDatabase"))
            {
                services.AddDbContext<IdentityUserContext>(options => options.UseInMemoryDatabase("LinkUpAppDb"));
            }
            else
            {
                var connectionString = config.GetConnectionString("DefaultConnection");
                services.AddDbContext<IdentityUserContext>(

                    (serviceProvider, opt) =>
                    {
                        opt.UseSqlServer(connectionString,
                        m => m.MigrationsAssembly(typeof(IdentityUserContext).Assembly.FullName));
                    },
                    contextLifetime: ServiceLifetime.Scoped,
                    optionsLifetime: ServiceLifetime.Scoped
                );
            }
            #endregion
        }
        #endregion
    }
}
