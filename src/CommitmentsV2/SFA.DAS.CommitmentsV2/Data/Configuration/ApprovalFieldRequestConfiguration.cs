using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SFA.DAS.CommitmentsV2.Models;

namespace SFA.DAS.CommitmentsV2.Data.Configuration;

public class ApprovalFieldRequestConfiguration : IEntityTypeConfiguration<ApprovalFieldRequest>
{
    public void Configure(EntityTypeBuilder<ApprovalFieldRequest> builder)
    {
        builder.ToTable("ApprovalFieldRequest")
            .HasKey("Id");

        //builder   //.HasMany(d => d.Items)
        //    .WithOne(x => x.ApprovalRequestId)
        //    .HasForeignKey(x => x.ApprovalRequestId)
        //    .OnDelete(DeleteBehavior.Cascade);
    }
}