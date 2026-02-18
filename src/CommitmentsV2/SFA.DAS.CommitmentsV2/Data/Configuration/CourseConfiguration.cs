using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SFA.DAS.CommitmentsV2.Models;

namespace SFA.DAS.CommitmentsV2.Data.Configuration;

public class CourseConfiguration : IEntityTypeConfiguration<Course>
{
    public void Configure(EntityTypeBuilder<Course> builder)
    {
        builder.ToTable("Course");
        builder.HasKey(x => x.LarsCode);

        builder.Property(x => x.LarsCode).HasColumnName("LarsCode").HasColumnType("varchar").HasMaxLength(20).IsRequired();
        builder.Property(x => x.Title).HasColumnName("Title").HasColumnType("varchar").HasMaxLength(500).IsRequired();
        builder.Property(x => x.Level).HasColumnName("Level").HasColumnType("varchar").HasMaxLength(20).IsRequired();
        builder.Property(x => x.LearningType).HasColumnName("LearningType").HasColumnType("varchar").HasMaxLength(50).IsRequired(false);
        builder.Property(x => x.MaxFunding).HasColumnName("MaxFunding").HasColumnType("int").IsRequired();
        builder.Property(x => x.EffectiveFrom).HasColumnName("EffectiveFrom").HasColumnType("DateTime").IsRequired(false);
        builder.Property(x => x.EffectiveTo).HasColumnName("EffectiveTo").HasColumnType("DateTime").IsRequired(false);
    }
}