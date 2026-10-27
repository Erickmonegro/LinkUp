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
    public class BattleshipMatchEntityConfiguration : IEntityTypeConfiguration<BattleshipMatch>
    {
        public void Configure(EntityTypeBuilder<BattleshipMatch> builder)
        {
            #region Basic configuration
            builder.HasKey(m => m.Id);
            builder.ToTable("BattleshipMatches");
            #endregion

            #region Property configurations
            builder.Property(m => m.Player1Id).IsRequired();
            builder.Property(m => m.Player2Id).IsRequired();
            builder.Property(m => m.CurrentTurnUserId).IsRequired();
            #endregion

            #region relationships
            builder.HasMany(m => m.Ships)
                .WithOne(s => s.BattleshipMatch)
                .HasForeignKey(s => s.BattleshipMatchId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(m => m.Attacks)
                .WithOne(a => a.Match)
                .HasForeignKey(a => a.MatchId)
                .OnDelete(DeleteBehavior.Cascade);
            #endregion
        }
    }
}
