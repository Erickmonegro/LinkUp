using LinkUpPro.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkUpPro.Infrastructure.Persistence.EntityConfigurations
{
    public class FriendshipEntityConfiguration : IEntityTypeConfiguration<Friendship>
    {
        public void Configure(EntityTypeBuilder<Friendship> builder)
        {
            #region Basic configuration
            builder.HasKey(f => f.Id);
            builder.ToTable("Friendships");
            #endregion

            #region Property configurations
            builder.Property(f => f.UserId1).IsRequired();
            builder.Property(f => f.UserId2).IsRequired();
            // Prevent duplicate friendships between the same two users
            builder.HasIndex(f => new { f.UserId1, f.UserId2 }).IsUnique();
            #endregion

            #region relationships
            #endregion
        }
    }
}
