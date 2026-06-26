using LinkUpPro.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkUpPro.Infrastructure.Persistence.EntityConfigurations
{
    public class FriendRequestEntityConfiguration : IEntityTypeConfiguration<FriendRequest>
    {
        public void Configure(EntityTypeBuilder<FriendRequest> builder)
        {
            #region Basic configuration
            builder.HasKey(fr => fr.Id);
            builder.ToTable("FriendRequests");
            #endregion

            #region Property configurations
            builder.Property(fr => fr.SenderId).IsRequired();
            builder.Property(fr => fr.ReceiverId).IsRequired();
            #endregion

            #region relationships
            #endregion
        }
    }
}
