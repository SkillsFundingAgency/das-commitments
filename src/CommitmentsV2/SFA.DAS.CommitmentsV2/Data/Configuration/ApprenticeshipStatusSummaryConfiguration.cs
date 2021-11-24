using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SFA.DAS.CommitmentsV2.Models;

namespace SFA.DAS.CommitmentsV2.Data.Configuration
{
    public class ApprenticeshipStatusSummaryConfiguration : IEntityTypeConfiguration<ApprenticeshipStatusSummary>
    {
        public void Configure(EntityTypeBuilder<ApprenticeshipStatusSummary> builder)
        {
            builder.HasKey(x => new { x.LegalEntityId, x.PaymentStatus });
            builder.Property(e => e.LegalEntityId).HasColumnName("LegalEntityId").HasColumnType("varchar").HasMaxLength(100).IsRequired();
            builder.Property(e => e.PaymentStatus).IsRequired().HasColumnType("smallint");
            builder.Property(e => e.LegalEntityOrganisationType).IsRequired().HasColumnType("smallint");
            builder.Property(e => e.Count).HasColumnType("int").IsRequired();
        }
    }
}
