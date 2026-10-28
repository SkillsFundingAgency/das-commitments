using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SFA.DAS.CommitmentsV2.Models;

namespace SFA.DAS.CommitmentsV2.Data.Configuration;

public class CocApprovalItemConfiguration : IEntityTypeConfiguration<CocApprovalItem>
{
    public void Configure(EntityTypeBuilder<CocApprovalItem> builder)
    {
        builder.ToTable("CocApprovalItem")
            .HasKey("Id");
        builder.Property(e => e.Created).HasDefaultValueSql("(getdate())").ValueGeneratedOnAddOrUpdate();
        builder.Property(e => e.ProviderAcknowledgedBy).HasMaxLength(255);
        builder.Property(e => e.EmployerAcknowledgedBy).HasMaxLength(255);

        builder.HasOne<Apprenticeship>()
            .WithMany(apprenticeship => apprenticeship.CocApprovalItems)
            .HasForeignKey(item => item.ApprenticeshipId)
            .HasPrincipalKey(apprenticeship => apprenticeship.Id);
    }
}