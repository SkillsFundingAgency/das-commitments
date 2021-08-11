using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SFA.DAS.CommitmentsV2.Models;

namespace SFA.DAS.CommitmentsV2.Data.Configuration
{
    public class OverlappingEmailConfiguration : IEntityTypeConfiguration<OverlappingEmail>
    {
        public void Configure(EntityTypeBuilder<OverlappingEmail> builder)
        {
            builder.Property(e => e.IsApproved).HasColumnType("bool");
            builder.Property(e => e.OverlapStatus).HasColumnType("int");
        }
    }
}