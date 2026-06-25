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
            #endregion

            #region Property configurations
            #endregion

            #region relationships
            #endregion
        }
    }
}
