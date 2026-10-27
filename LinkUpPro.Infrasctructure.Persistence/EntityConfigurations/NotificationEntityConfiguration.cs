using LinkUpPro.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkUpPro.Infrastructure.Persistence.EntityConfigurations
{
    public class NotificationEntityConfiguration : IEntityTypeConfiguration<Notification>
    {
        public void Configure(EntityTypeBuilder<Notification> builder)
        {
            #region Basic configuration
            builder.HasKey(n => n.Id);
            builder.ToTable("Notifications");
            #endregion

            #region Property configurations
            builder.Property(n => n.Description).IsRequired().HasMaxLength(500);
            builder.Property(n => n.ReceiverId).IsRequired();
            builder.Property(n => n.TriggerUserId).IsRequired();
            #endregion

            #region relationships
            #endregion
        }
    }
}
