using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SFA.DAS.CommitmentsV2.Models;

namespace SFA.DAS.CommitmentsV2.Data.Configuration
{
    class ApprenticeshipBuilder : IEntityTypeConfiguration<Apprenticeship>
    {
        public void Configure(EntityTypeBuilder<Apprenticeship> builder)
        {
            builder.HasBaseType<ApprenticeshipBase>();

            builder.Property(e => e.PauseDate).HasColumnType("date");
            builder.Property(e => e.StopDate).HasColumnType("date");
        }
    }
}
