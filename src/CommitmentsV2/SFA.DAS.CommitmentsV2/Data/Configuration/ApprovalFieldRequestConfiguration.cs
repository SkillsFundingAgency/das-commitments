using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SFA.DAS.CommitmentsV2.Models;

namespace SFA.DAS.CommitmentsV2.Data.Configuration;

public class ApprovalFieldRequestConfiguration : IEntityTypeConfiguration<ApprovalFieldRequest>
{
    public void Configure(EntityTypeBuilder<ApprovalFieldRequest> builder)
    {
        builder.ToTable("ApprovalFieldRequest")
            .HasKey("Id");
    }
}