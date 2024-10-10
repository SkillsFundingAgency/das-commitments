using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SFA.DAS.CommitmentsV2.Models;

namespace SFA.DAS.CommitmentsV2.Data.Configuration;

public class ApprenticeshipBuilder : IEntityTypeConfiguration<Apprenticeship>
{
    public void Configure(EntityTypeBuilder<Apprenticeship> builder)
    {
        builder.HasBaseType<ApprenticeshipBase>();

        builder.Property(e => e.PauseDate).HasColumnType("date");
        builder.Property(e => e.StopDate).HasColumnType("date");

        // Fix for "Could not save changes because the target table has database triggers" exception.
        // https://learn.microsoft.com/en-gb/ef/core/what-is-new/ef-core-7.0/breaking-changes?tabs=v7#sqlserver-tables-with-triggers
        builder.ToTable(x => x.UseSqlOutputClause(false));
    }
}