using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SFA.DAS.CommitmentsV2.Models;

namespace SFA.DAS.CommitmentsV2.Data.Configuration;

public class JobProgressConfiguration : IEntityTypeConfiguration<JobProgress>
{
    public void Configure(EntityTypeBuilder<JobProgress> builder)
    {
        builder.HasKey(e => e.Lock);

        builder.Property(e => e.Lock)
            .HasMaxLength(1)
            .IsUnicode(false)
            .HasDefaultValueSql("('X')");

        builder.Property(e => e.AddEpaLastSubmissionEventId).HasColumnName("AddEpa_LastSubmissionEventId");
        builder.Property(e => e.IntTestSchemaVersion).HasColumnName("IntTest_SchemaVersion");
    }
}