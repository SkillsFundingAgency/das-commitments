using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SFA.DAS.CommitmentsV2.Models;

namespace SFA.DAS.CommitmentsV2.Data.Configuration;

public class StandardConfiguration : IEntityTypeConfiguration<Standard>
{
    public void Configure(EntityTypeBuilder<Standard> builder)
    {
        builder.ToTable("Standard");
        builder.HasKey(x=> x.StandardUId);

        builder.Property(x => x.StandardUId).HasColumnName("StandardUId").HasColumnType("varchar").HasMaxLength(20).IsRequired();
        builder.Property(x => x.LarsCode).HasColumnName("LarsCode").HasColumnType("int").IsRequired().ValueGeneratedNever();
        builder.Property(x => x.IFateReferenceNumber).HasColumnName("IFateReferenceNumber").HasColumnType("varchar").HasMaxLength(10).IsRequired(false);
        builder.Property(x => x.Version).HasColumnName("Version").HasColumnType("varchar").HasMaxLength(10).IsRequired(false);
        builder.Property(x => x.Title).HasColumnName("Title").HasColumnType("varchar").HasMaxLength(500).IsRequired(false);
        builder.Property(x => x.Duration).HasColumnName("Duration").HasColumnType("int").IsRequired();
        builder.Property(x => x.Level).HasColumnName("Level").HasColumnType("tinyint").IsRequired();
        builder.Property(x => x.MaxFunding).HasColumnName("MaxFunding").HasColumnType("int").IsRequired();
        builder.Property(x => x.EffectiveFrom).HasColumnName("EffectiveFrom").HasColumnType("DateTime").IsRequired(false);
        builder.Property(x => x.EffectiveTo).HasColumnName("EffectiveTo").HasColumnType("DateTime").IsRequired(false);
        builder.Property(x => x.StandardPageUrl).HasColumnName("StandardPageUrl").HasColumnType("varchar").HasMaxLength(500).IsRequired(false);
        builder.Property(x => x.IsLatestVersion).HasColumnName("IsLatestVersion").HasColumnType("bit");
        builder.Property(x => x.ApprenticeshipType).HasColumnName("ApprenticeshipType").HasColumnType("varchar").HasMaxLength(50);

        builder.HasMany(c => c.FundingPeriods)
            .WithOne(c => c.Standard)
            .HasPrincipalKey(c => c.LarsCode)
            .HasForeignKey(c => c.Id).Metadata.DeleteBehavior = DeleteBehavior.Restrict;

        builder.HasMany(c => c.Options);

        builder.HasIndex(c => c.StandardUId);
    }
}