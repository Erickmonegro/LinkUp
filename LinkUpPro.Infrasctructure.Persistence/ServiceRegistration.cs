using LinkUpPro.Core.Domain.Interfaces;
using LinkUpPro.Infrastructure.Persistence.Contexts;
using LinkUpPro.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LinkUpPro.Infrastructure.Persistence
{
    public static class ServiceRegistration
    {
        public static void ServiceRegistrationIOC(this IServiceCollection services, IConfiguration config)
        {
            #region Contexts
            if (bool.Parse(config["UseInMemoryDatabase"] ?? "false"))
            {
                services.AddDbContext<LinkUpAppContext>(options =>
                    options.UseInMemoryDatabase("LinkUpDb"));
            }
            else
            {
                var connectionString = config.GetConnectionString("DefaultConnection");
                services.AddDbContext<LinkUpAppContext>(
                  (serviceProvider, opt) =>
                  {
                      opt.EnableSensitiveDataLogging();
                      opt.UseSqlServer(connectionString,
                      m => m.MigrationsAssembly(typeof(LinkUpAppContext).Assembly.FullName));
                  },
                    contextLifetime: ServiceLifetime.Scoped,
                    optionsLifetime: ServiceLifetime.Scoped
                 );
            }
            #endregion

            #region Repositories IOC
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            services.AddScoped<IAttackRepository, AttackRepository>();
            services.AddScoped<IShipPositionRepository, ShipPositionRepository>();
            services.AddScoped<IBattleshipMatchRepository, BattleshipMatchRepository>();
            services.AddScoped<IPostRepository, PostRepository>();
            services.AddScoped<ICommentRepository, CommentRepository>();
            services.AddScoped<IFriendshipRepository, FriendshipRepository>();
            services.AddScoped<IFriendRequestRepository, FriendRequestRepository>();
            services.AddScoped<INotificationRepository, NotificationRepository>();
            services.AddScoped<IReactionRepository, ReactionRepository>();
            #endregion
        }
    }
}
