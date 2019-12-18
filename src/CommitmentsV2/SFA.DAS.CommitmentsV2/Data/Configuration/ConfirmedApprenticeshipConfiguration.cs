using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SFA.DAS.CommitmentsV2.Models;

namespace SFA.DAS.CommitmentsV2.Data.Configuration
{
    public class ConfirmedApprenticeshipConfiguration : IEntityTypeConfiguration<ApprovedApprenticeship>
    {
        public void Configure(EntityTypeBuilder<ApprovedApprenticeship> builder)
        {
            builder.HasBaseType<ApprenticeshipBase>();
        }
    }
}