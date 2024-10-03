using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SFA.DAS.CommitmentsV2.Models;

namespace SFA.DAS.CommitmentsV2.Data.Configuration;

public class StandardOptionConfiguration : IEntityTypeConfiguration<StandardOption>
{
    public void Configure(EntityTypeBuilder<StandardOption> builder)
    {
        builder.ToTable("StandardOption");

        builder.HasKey(x => new { x.StandardUId, x.Option });

        builder.Property(x => x.StandardUId).HasColumnName("StandardUId").HasColumnType("varchar").HasMaxLength(20).IsRequired();
        builder.Property(x => x.Option).HasColumnName("Option").HasColumnType("varchar").HasMaxLength(200).IsRequired();

        builder.HasOne(x => x.Standard)
            .WithMany(x => x.Options)
            .HasForeignKey(s => s.StandardUId);
    }
}