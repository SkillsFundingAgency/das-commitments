using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SFA.DAS.CommitmentsV2.Models;

namespace SFA.DAS.CommitmentsV2.Data.Configuration
{
    public class ConfirmedApprenticeshipConfiguration : IEntityTypeConfiguration<ConfirmedApprenticeship>
    {
        public void Configure(EntityTypeBuilder<ConfirmedApprenticeship> builder)
        {
            builder.HasBaseType<Apprenticeship>();
        }
    }
}