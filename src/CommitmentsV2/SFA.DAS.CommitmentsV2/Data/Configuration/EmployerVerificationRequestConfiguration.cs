using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SFA.DAS.CommitmentsV2.Models;

namespace SFA.DAS.CommitmentsV2.Data.Configuration;

public class EmployerVerificationRequestConfiguration : IEntityTypeConfiguration<EmployerVerificationRequest>
{
    public void Configure(EntityTypeBuilder<EmployerVerificationRequest> builder)
    {
        builder.ToTable("EmployerVerificationRequest");
        builder.HasKey(e => e.ApprenticeshipId);

        builder.Property(e => e.Created).HasColumnType("datetime2");
        builder.Property(e => e.Updated).HasColumnType("datetime2");
        builder.Property(e => e.LastCheckedDate).HasColumnType("datetime2");
        builder.Property(e => e.Status).HasColumnType("smallint");
        builder.Property(e => e.Notes).HasMaxLength(1000);

        builder
            .HasOne(e => e.Apprenticeship)
            .WithOne(a => a.EmployerVerificationRequest)
            .HasForeignKey<EmployerVerificationRequest>(e => e.ApprenticeshipId);
    }
}
