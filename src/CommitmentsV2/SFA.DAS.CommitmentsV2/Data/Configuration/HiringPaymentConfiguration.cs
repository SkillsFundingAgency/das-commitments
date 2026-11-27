using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SFA.DAS.CommitmentsV2.Models;

namespace SFA.DAS.CommitmentsV2.Data.Configuration;

public class HiringPaymentConfiguration : IEntityTypeConfiguration<HiringPayment>
{
    public void Configure(EntityTypeBuilder<HiringPayment> builder)
    {
        builder.ToTable("HiringPayment");
        builder.HasKey(e => e.ApprenticeshipId);

        builder.Property(e => e.Uln).HasMaxLength(50);
        builder.Property(e => e.Created).HasColumnType("datetime2");
        builder.Property(e => e.Updated).HasColumnType("datetime2");
        builder.Property(e => e.Status).HasColumnType("smallint");

        builder
            .HasOne(e => e.Apprenticeship)
            .WithOne()
            .HasForeignKey<HiringPayment>(e => e.ApprenticeshipId);
    }
}