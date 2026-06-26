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
    public class ShipPositionEntityConfiguration : IEntityTypeConfiguration<ShipPosition>
    {
        public void Configure(EntityTypeBuilder<ShipPosition> builder)
        {
            #region Basic configuration
            builder.HasKey(s => s.Id);
            builder.ToTable("ShipPositions");
            #endregion

            #region Property configurations
            builder.Property(s => s.UserId).IsRequired();
            builder.Property(s => s.BattleshipMatchId).IsRequired();
            builder.Property(s => s.ShipSize).IsRequired();
            builder.Property(s => s.StartX).IsRequired();
            builder.Property(s => s.StartY).IsRequired();
            #endregion

            #region relationships
            #endregion
        }
    }
}
