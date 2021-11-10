using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SFA.DAS.CommitmentsV2.Models;

namespace SFA.DAS.CommitmentsV2.Data.Configuration
{
    public class DraftApprenticeshipConfiguration : IEntityTypeConfiguration<DraftApprenticeship>
    {
        public void Configure(EntityTypeBuilder<DraftApprenticeship> builder)
        {
            builder.HasBaseType<ApprenticeshipBase>();
            builder.Property(e => e.Id).ValueGeneratedNever();
        }
    }
}