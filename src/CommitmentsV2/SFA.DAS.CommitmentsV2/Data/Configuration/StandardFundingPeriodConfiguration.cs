using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SFA.DAS.CommitmentsV2.Models;

namespace SFA.DAS.CommitmentsV2.Data.Configuration
{
    public class StandardFundingPeriodConfiguration : IEntityTypeConfiguration<StandardFundingPeriod>
    {
        public void Configure(EntityTypeBuilder<StandardFundingPeriod> builder)
        {
            
            builder.ToTable("StandardFunding").HasKey(c=>new{c.Id,c.EffectiveFrom});
            builder.Property(x => x.Id).HasColumnName("Id").HasColumnType("int").IsRequired().ValueGeneratedNever();
            builder.Property(x => x.EffectiveFrom).HasColumnName("EffectiveFrom").HasColumnType("DateTime").IsRequired();
            builder.Property(x => x.EffectiveTo).HasColumnName("EffectiveTo").HasColumnType("DateTime").IsRequired(false);
            builder.Property(x => x.FundingCap).HasColumnName("FundingCap").HasColumnType("int").IsRequired();
        }
    }
}