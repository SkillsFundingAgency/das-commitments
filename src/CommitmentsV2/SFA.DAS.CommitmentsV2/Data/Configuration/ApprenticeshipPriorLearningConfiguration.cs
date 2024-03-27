using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SFA.DAS.CommitmentsV2.Models;

namespace SFA.DAS.CommitmentsV2.Data.Configuration
{
    public class ApprenticeshipPriorLearningConfiguration : IEntityTypeConfiguration<ApprenticeshipPriorLearning>
    {
        public void Configure(EntityTypeBuilder<ApprenticeshipPriorLearning> builder)
        {
            builder
                .ToTable("ApprenticeshipPriorLearning")
                .HasKey("ApprenticeshipId");

            builder
                .HasOne(d => d.Apprenticeship)
                .WithOne(p => p.PriorLearning)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}