using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SFA.DAS.CommitmentsV2.Models;

namespace SFA.DAS.CommitmentsV2.Data.Configuration
{
    public class FrameworkConfiguration : IEntityTypeConfiguration<Framework>
    {
        public void Configure(EntityTypeBuilder<Framework> builder)
        {
            builder.ToTable("Framework");
            builder.HasKey(x=> x.Id);

            builder.Property(x => x.Id).HasColumnName("Id").HasColumnType("varchar").HasMaxLength(25).IsRequired().ValueGeneratedNever();
            builder.Property(x => x.FrameworkCode).HasColumnName("FrameworkCode").HasColumnType("int").IsRequired();
            builder.Property(x => x.FrameworkName).HasColumnName("FrameworkName").HasColumnType("varchar").HasMaxLength(500).IsRequired();
            builder.Property(x => x.Level).HasColumnName("Level").HasColumnType("tinyint").IsRequired();
            builder.Property(x => x.PathwayCode).HasColumnName("PathwayCode").HasColumnType("int").IsRequired();
            builder.Property(x => x.PathwayName).HasColumnName("PathwayName").HasColumnType("varchar").HasMaxLength(500).IsRequired();
            builder.Property(x => x.ProgrammeType).HasColumnName("ProgrammeType").HasColumnType("int").IsRequired();
            builder.Property(x => x.Title).HasColumnName("Title").HasColumnType("varchar").HasMaxLength(500).IsRequired(false);
            builder.Property(x => x.Duration).HasColumnName("Duration").HasColumnType("int").IsRequired();
            builder.Property(x => x.MaxFunding).HasColumnName("MaxFunding").HasColumnType("int").IsRequired();
            builder.Property(x => x.EffectiveFrom).HasColumnName("EffectiveFrom").HasColumnType("DateTime").IsRequired(false);
            builder.Property(x => x.EffectiveTo).HasColumnName("EffectiveTo").HasColumnType("DateTime").IsRequired(false);
            
            builder.HasMany(c => c.FundingPeriods)
                .WithOne(c=>c.Framework)
                .HasPrincipalKey(c => c.Id)
                .HasForeignKey(c => c.Id).Metadata.DeleteBehavior = DeleteBehavior.Restrict;
            
            builder.HasIndex(c => c.Id);
        }
    }
}