using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SFA.DAS.CommitmentsV2.Models;

namespace SFA.DAS.CommitmentsV2.Data.Configuration
{
    public class ApprenticeshipConfirmationStatusConfiguration : IEntityTypeConfiguration<ApprenticeshipConfirmationStatus>
    {
        public void Configure(EntityTypeBuilder<ApprenticeshipConfirmationStatus> builder)
        {
            builder.ToTable("ApprenticeshipConfirmationStatusWithSort")
                .HasKey(e => e.Id);
            builder.Property(e => e.CommitmentsApprovedOn).HasColumnType("datetime");
            builder.Property(e => e.ConfirmationOverdueOn).HasColumnType("datetime");
            builder.Property(e => e.ApprenticeshipConfirmedOn).HasColumnType("datetime");
            builder.Property(e => e.ConfirmationStatusSort).HasColumnType("varchar(1)");

            builder.HasOne(d => d.Apprenticeship)
                .WithOne(p => p.ApprenticeshipConfirmationStatus);
        }
    }
}