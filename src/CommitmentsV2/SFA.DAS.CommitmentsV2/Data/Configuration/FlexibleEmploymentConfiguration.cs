using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SFA.DAS.CommitmentsV2.Models;

namespace SFA.DAS.CommitmentsV2.Data.Configuration
{
    public class FlexibleEmploymentConfiguration : IEntityTypeConfiguration<FlexibleEmployment>
    {
        public void Configure(EntityTypeBuilder<FlexibleEmployment> builder)
        {
            builder
                .ToTable("ApprenticeshipFlexibleEmployment")
                .HasKey("ApprenticeshipId");

            builder
                .Property(e => e.EmploymentPrice)
                .HasColumnType("decimal(18, 0)");

            builder
                .Property(e => e.EmploymentEndDate)
                .HasColumnType("datetime");

            builder
                .HasOne(d => d.Apprenticeship)
                .WithOne(p => p.FlexibleEmployment);
        }
    }
}