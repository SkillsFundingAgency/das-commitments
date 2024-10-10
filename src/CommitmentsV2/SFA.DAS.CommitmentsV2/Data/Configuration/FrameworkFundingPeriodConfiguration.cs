using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SFA.DAS.CommitmentsV2.Models;

namespace SFA.DAS.CommitmentsV2.Data.Configuration;

public class FrameworkFundingPeriodConfiguration : IEntityTypeConfiguration<FrameworkFundingPeriod>
{
    public void Configure(EntityTypeBuilder<FrameworkFundingPeriod> builder)
    {
        builder.ToTable("FrameworkFunding").HasKey(c=>new{c.Id,c.EffectiveFrom});
        builder.Property(x => x.Id).HasColumnName("Id").HasColumnType("varchar").HasMaxLength(25).IsRequired();
        builder.Property(x => x.EffectiveFrom).HasColumnName("EffectiveFrom").HasColumnType("DateTime").IsRequired();
        builder.Property(x => x.EffectiveTo).HasColumnName("EffectiveTo").HasColumnType("DateTime").IsRequired(false);
        builder.Property(x => x.FundingCap).HasColumnName("FundingCap").HasColumnType("int").IsRequired();
    }
}