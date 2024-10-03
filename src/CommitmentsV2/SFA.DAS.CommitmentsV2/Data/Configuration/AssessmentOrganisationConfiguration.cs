using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SFA.DAS.CommitmentsV2.Models;

namespace SFA.DAS.CommitmentsV2.Data.Configuration;

public class AssessmentOrganisationConfiguration : IEntityTypeConfiguration<AssessmentOrganisation>
{
    public void Configure(EntityTypeBuilder<AssessmentOrganisation> builder)
    {
        builder.ToTable("AssessmentOrganisation");

        builder.Property(e => e.EpaOrgId)
            .IsRequired()
            .HasColumnName("EPAOrgId")
            .HasMaxLength(7)
            .IsUnicode(false);

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(128);
    }
}