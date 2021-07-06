using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SFA.DAS.CommitmentsV2.Models;

namespace SFA.DAS.CommitmentsV2.Data.Configuration
{
    public class ApprenticeshipConfirmationStatusConfiguration : IEntityTypeConfiguration<ApprenticeshipConfirmationStatus>
    {
        public void Configure(EntityTypeBuilder<ApprenticeshipConfirmationStatus> builder)
        {
            builder.ToTable("ApprenticeshipConfirmationStatusWithSort")
                .HasKey(e => e.ApprenticeshipId);
            builder.Property(e => e.CommitmentsApprovedOn).HasColumnType("datetime");
            builder.Property(e => e.ConfirmationOverdueOn).HasColumnType("datetime");
            builder.Property(e => e.ApprenticeshipConfirmedOn).HasColumnType("datetime");
            var sortCol = builder.Property(e => e.ConfirmationStatusSort).HasColumnType("varchar(1)");
            sortCol.Metadata.BeforeSaveBehavior = PropertySaveBehavior.Ignore;
            sortCol.Metadata.AfterSaveBehavior = PropertySaveBehavior.Ignore;

            builder.HasOne(d => d.Apprenticeship)
                .WithOne(p => p.ApprenticeshipConfirmationStatus);
        }
    }
}