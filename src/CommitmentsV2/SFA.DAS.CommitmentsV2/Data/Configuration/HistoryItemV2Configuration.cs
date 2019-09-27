using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SFA.DAS.CommitmentsV2.Models;

namespace SFA.DAS.CommitmentsV2.Data.Configuration
{
    public class HistoryItemV2Configuration : IEntityTypeConfiguration<HistoryItemV2>
    {
        public void Configure(EntityTypeBuilder<HistoryItemV2> builder)
        {
            builder.ToTable("HistoryV2");
            builder.Property(h => h.EntityType).HasMaxLength(255).IsRequired();
        }
    }
}