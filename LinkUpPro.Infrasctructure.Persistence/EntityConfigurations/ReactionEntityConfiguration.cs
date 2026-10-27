using LinkUpPro.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;


namespace LinkUpPro.Infrastructure.Persistence.EntityConfigurations
{
    public class ReactionEntityConfiguration : IEntityTypeConfiguration<Reaction>
    {
        public void Configure(EntityTypeBuilder<Reaction> builder)
        {
            #region Basic configuration
            builder.HasKey(r => r.Id);
            builder.ToTable("Reactions");
            #endregion

            #region Property configurations
            builder.Property(r => r.UserId).IsRequired();
            builder.Property(r => r.PostId).IsRequired();
            #endregion

            #region relationships
            #endregion
        }
    }
}
