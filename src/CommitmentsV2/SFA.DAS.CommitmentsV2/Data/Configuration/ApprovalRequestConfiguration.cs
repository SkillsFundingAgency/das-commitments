using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SFA.DAS.CommitmentsV2.Models;

namespace SFA.DAS.CommitmentsV2.Data.Configuration;

public class ApprovalRequestConfiguration : IEntityTypeConfiguration<ApprovalRequest>
{
    public void Configure(EntityTypeBuilder<ApprovalRequest> builder)
    {
        builder.ToTable("ApprovalRequest")
            .HasKey("Id");
        builder.Property(e => e.Created).HasDefaultValueSql("(getdate())");

        builder.HasMany(d => d.Items)
            .WithOne(x => x.ApprovalRequest)
            .HasForeignKey(x => x.ApprovalRequestId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ar => ar.Apprenticeship)
            .WithMany(a => a.ApprovalRequests)
            .HasForeignKey(ar => ar.ApprenticeshipId)
            .HasPrincipalKey(a => a.Id);
    }
}