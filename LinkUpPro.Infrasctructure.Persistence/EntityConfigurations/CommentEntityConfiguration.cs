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
    public class CommentEntityConfiguration : IEntityTypeConfiguration<Comment>
    {
        public void Configure(EntityTypeBuilder<Comment> builder)
        {
            #region Basic configuration
            builder.HasKey(c => c.Id);
            builder.ToTable("Comments");
            #endregion

            #region Property configurations
            builder.Property(c => c.Content).IsRequired().HasMaxLength(500);
            builder.Property(c => c.AuthorId).IsRequired();
            builder.Property(c => c.PostId).IsRequired();
            #endregion

            #region relationships
            builder.HasMany(c => c.Replies)
                .WithOne(r => r.ParentComment)
                .HasForeignKey(r => r.ParentCommentId)
                .OnDelete(DeleteBehavior.Restrict);
            #endregion
        }
    }
}
