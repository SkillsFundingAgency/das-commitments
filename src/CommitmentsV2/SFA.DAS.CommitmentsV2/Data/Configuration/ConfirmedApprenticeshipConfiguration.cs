using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SFA.DAS.CommitmentsV2.Models;

namespace SFA.DAS.CommitmentsV2.Data.Configuration
{
    public class ConfirmedApprenticeshipConfiguration : IEntityTypeConfiguration<Apprenticeship>
    {
        public void Configure(EntityTypeBuilder<Apprenticeship> builder)
        {
            builder.HasBaseType<ApprenticeshipBase>();
        }
    }
}