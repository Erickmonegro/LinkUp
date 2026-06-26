using LinkUpPro.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkUpPro.Infrastructure.Persistence.EntityConfigurations
{
    public class PostEntityConfiguration : IEntityTypeConfiguration<Post>
    {
        public void Configure(EntityTypeBuilder<Post> builder)
        {
            #region Basic configuration
            builder.HasKey(p => p.Id);
            builder.ToTable("Posts");
            #endregion

            #region Property configurations
            builder.Property(p => p.TextContent).IsRequired().HasMaxLength(1000);
            builder.Property(p => p.AuthorId).IsRequired();
            #endregion

            #region relationships
            builder.HasMany(p => p.Comments)
                .WithOne(c => c.Post)
                .HasForeignKey(c => c.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(p => p.Reactions)
                .WithOne(r => r.Post)
                .HasForeignKey(r => r.PostId)
                .OnDelete(DeleteBehavior.Cascade);
            #endregion
        }
    }
}
