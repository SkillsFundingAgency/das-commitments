using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SFA.DAS.CommitmentsV2.Models;

namespace SFA.DAS.CommitmentsV2.Data.Configuration;

public class DataLockStatusConfiguration : IEntityTypeConfiguration<DataLockStatus>
{
    public void Configure(EntityTypeBuilder<DataLockStatus> builder)
    {
        builder.ToTable("DataLockStatus");

        builder.Property(e => e.DataLockEventDatetime).HasColumnType("datetime");
        builder.Property(e => e.EventStatus).HasColumnType("tinyint").HasDefaultValueSql("((1))");
        builder.Property(e => e.TriageStatus).HasColumnType("tinyint");
        builder.Property(e => e.Expired).HasColumnType("datetime");
        builder.Property(e => e.IlrActualStartDate).HasColumnType("datetime");
        builder.Property(e => e.IlrEffectiveFromDate).HasColumnType("datetime");
        builder.Property(e => e.IlrPriceEffectiveToDate).HasColumnType("datetime");
        builder.Property(e => e.IlrTotalCost).HasColumnType("decimal(18, 0)");
        builder.Property(e => e.IlrTrainingCourseCode).HasMaxLength(20);
        builder.Property(e => e.IlrTrainingType).HasColumnType("tinyint");

        builder.Property(e => e.PriceEpisodeIdentifier)
            .IsRequired()
            .HasMaxLength(25);

        builder.HasOne(d => d.Apprenticeship)
            .WithMany(p => p.DataLockStatus)
            .HasForeignKey(d => d.ApprenticeshipId)
            .OnDelete(DeleteBehavior.ClientSetNull);
    }
}