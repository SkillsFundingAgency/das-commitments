using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SFA.DAS.CommitmentsV2.Models;

namespace SFA.DAS.CommitmentsV2.Data.Configuration
{
    public class StandardConfiguration : IEntityTypeConfiguration<Standard>
    {
        public void Configure(EntityTypeBuilder<Standard> builder)
        {
            builder.ToTable("Standard");
            builder.HasKey(x=> x.Id);

            builder.Property(x => x.Id).HasColumnName("Id").HasColumnType("int").IsRequired().ValueGeneratedNever();
            builder.Property(x => x.Title).HasColumnName("Title").HasColumnType("varchar").HasMaxLength(500).IsRequired(false);
            builder.Property(x => x.Duration).HasColumnName("Duration").HasColumnType("int").IsRequired();
            builder.Property(x => x.Level).HasColumnName("Level").HasColumnType("tinyint").IsRequired();
            builder.Property(x => x.MaxFunding).HasColumnName("MaxFunding").HasColumnType("int").IsRequired();
            builder.Property(x => x.EffectiveFrom).HasColumnName("EffectiveFrom").HasColumnType("DateTime").IsRequired(false);
            builder.Property(x => x.EffectiveTo).HasColumnName("EffectiveTo").HasColumnType("DateTime").IsRequired(false);
            
            builder.HasMany(c => c.FundingPeriods)
                .WithOne(c=>c.Standard)
                .HasPrincipalKey(c => c.Id)
                .HasForeignKey(c => c.Id).Metadata.DeleteBehavior = DeleteBehavior.Restrict;

            builder.HasIndex(c => c.Id);
        }
    }
}