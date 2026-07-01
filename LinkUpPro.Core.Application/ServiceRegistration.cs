using LinkUpPro.Core.Application.Interfaces;
using LinkUpPro.Core.Application.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;


namespace LinkUpPro.Core.Application
{
    public static class ServiceRegistration
    {
        public static void AddApplicationLayerIoc(this IServiceCollection services)
        {
            #region Configurations
            services.AddAutoMapper(Assembly.GetExecutingAssembly());
            #endregion

            #region Services IOC
            services.AddScoped<IFriendshipService, FriendshipService>();
            services.AddScoped<ICommentService, CommentService>();
            services.AddScoped<IPostService, PostService>();
            services.AddScoped<INotificationService, NotificationService>();
            services.AddScoped<IFriendRequestService, FriendRequestService>();
            services.AddScoped<IReactionService, ReactionService>();
            services.AddScoped<IShipPositionService, ShipPositionService>();
            services.AddScoped<IAttackService, AttackService>();
            services.AddScoped<IBattleshipMatchService, BattleshipMatchService>();
            #endregion
        }
    }
}
