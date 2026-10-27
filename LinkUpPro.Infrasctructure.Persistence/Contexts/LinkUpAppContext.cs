using LinkUpPro.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LinkUpPro.Infrastructure.Persistence.Contexts
{
    public class LinkUpAppContext : DbContext
    {
        public LinkUpAppContext(DbContextOptions<LinkUpAppContext> options) : base(options) { }
    
        public DbSet<Attack> Attack { get; set; }
        public DbSet<BattleshipMatch> BattleshipMatches { get; set; }
        public DbSet<ShipPosition> ShipsPosition { get; set; }
        public DbSet<Comment> Comment { get; set; }
        public DbSet<FriendRequest> FriendRequest { get; set; }
        public DbSet<Friendship> Friendship { get; set; }
        public DbSet<Notification> Notification { get; set; }
        public DbSet<Post> Post { get; set; }
        public DbSet<Reaction> Reaction { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }

    }
}
