using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SFA.DAS.CommitmentsV2.Models;

namespace SFA.DAS.CommitmentsV2.Data.Configuration;

public class ChangeOfPartyRequestConfiguration : IEntityTypeConfiguration<ChangeOfPartyRequest>
{
    public void Configure(EntityTypeBuilder<ChangeOfPartyRequest> builder)
    {
        builder.ToTable("ChangeOfPartyRequest");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.RowVersion).IsConcurrencyToken().ValueGeneratedOnAddOrUpdate();

        builder.HasOne(d => d.Apprenticeship)
            .WithMany(p => p.ChangeOfPartyRequests)
            .HasForeignKey(d => d.ApprenticeshipId)
            .OnDelete(DeleteBehavior.ClientSetNull);

        builder.HasOne(d => d.AccountLegalEntity)
            .WithMany(p => p.ChangeOfPartyRequests)
            .HasForeignKey(d => d.AccountLegalEntityId)
            .OnDelete(DeleteBehavior.ClientSetNull);

        builder.HasOne(x => x.Cohort)
            .WithOne(c => c.ChangeOfPartyRequest)
            .HasForeignKey<ChangeOfPartyRequest>(c => c.CohortId)
            .OnDelete(DeleteBehavior.ClientSetNull);
    }
}