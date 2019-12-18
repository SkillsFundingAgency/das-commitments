using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SFA.DAS.CommitmentsV2.Models;

namespace SFA.DAS.CommitmentsV2.Data.Configuration
{
    class ApprovedApprenticeshipBuilder : IEntityTypeConfiguration<ApprovedApprenticeship>
    {
        public void Configure(EntityTypeBuilder<ApprovedApprenticeship> builder)
        {
            builder.HasBaseType<Apprenticeship>();

            builder.Property(e => e.PauseDate).HasColumnType("date");
            builder.Property(e => e.StopDate).HasColumnType("date");
        }
    }
}
