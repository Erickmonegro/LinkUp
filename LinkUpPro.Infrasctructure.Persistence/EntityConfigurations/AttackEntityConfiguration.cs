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
    public class AttackEntityConfiguration : IEntityTypeConfiguration<Attack>
    {
        public void Configure(EntityTypeBuilder<Attack> builder)
        {
            #region Basic configuration
            builder.HasKey(a => a.Id);
            builder.ToTable("Attacks");
            #endregion

            #region Property configurations
            builder.Property(a => a.AttackerId).IsRequired();
            builder.Property(a => a.MatchId).IsRequired();
            builder.Property(a => a.X).IsRequired();
            builder.Property(a => a.Y).IsRequired();
            #endregion

            #region relationships
            #endregion
        }
    }
}
