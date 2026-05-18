using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SFA.DAS.CommitmentsV2.Models;

namespace SFA.DAS.CommitmentsV2.Data.Configuration;

public class LearningChangeHistoryConfiguration : IEntityTypeConfiguration<LearningChangeHistory>
{
    public void Configure(EntityTypeBuilder<LearningChangeHistory> builder)
    {
        builder.ToTable("LearningChangeHistory")
            .HasKey(x => x.Id);

        builder.Property(x => x.Source).
            IsRequired();

        builder.Property(x => x.Description)
            .IsRequired()
            .HasMaxLength(1024)
            .HasColumnType("nvarchar(1024)");

        builder.Property(x => x.UserId)
            .IsRequired(false);

        builder.Property(x => x.ApprenticeshipId)
            .IsRequired();

        builder.Property(x => x.LearnerName)
            .IsRequired()
            .HasMaxLength(200)
            .HasColumnType("nvarchar(200)");

        builder.Property(x => x.LearnerKey)
            .IsRequired(false);

        builder.Property(x => x.Created)
            .IsRequired()
            .HasColumnType("datetime2(0)");

        builder.Property(x => x.AppliedDate)
            .IsRequired()
            .HasColumnType("datetime2(0)");

        builder.Property(x => x.AccountId)
            .IsRequired();

        builder.Property(x => x.UKPRN)
            .IsRequired();

        builder.Property(x => x.ProviderName)
            .IsRequired()
            .HasMaxLength(100)
            .HasColumnType("nvarchar(100)");

        builder.Property(x => x.EmployerName)
           .IsRequired()
           .HasMaxLength(100)
           .HasColumnType("nvarchar(100)");

        builder.HasIndex(x => x.AccountId)
            .HasDatabaseName("LearningChangeHistory_AccountId_IDX");

        builder.HasIndex(x => x.AppliedDate)
        .HasDatabaseName("LearningChangeHistory_AppliedDate_IDX");

        builder.HasIndex(x => x.ApprenticeshipId)
            .HasDatabaseName("LearningChangeHistory_ApprenticeshipId_IDX");

        builder.HasIndex(x => x.Created)
            .HasDatabaseName("LearningChangeHistory_Created_IDX");
    }
}