using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SFA.DAS.CommitmentsV2.Models;

namespace SFA.DAS.CommitmentsV2.Data.Configuration;

public class CohortConfiguration : IEntityTypeConfiguration<Cohort>
{
    public void Configure(EntityTypeBuilder<Cohort> builder)
    {
        builder.ToTable("Commitment");

        builder.Property(p => p.RowVersion).IsConcurrencyToken().ValueGeneratedOnAddOrUpdate();
            
        builder.HasQueryFilter(x => EF.Property<bool>(x, "IsDeleted") == false);

        builder.Property(e => e.CreatedOn).HasColumnType("datetime");
        builder.Property(e => e.LastUpdatedByEmployerEmail).HasMaxLength(255);
        builder.Property(e => e.LastUpdatedByEmployerName).HasMaxLength(255);
        builder.Property(e => e.LastUpdatedByProviderEmail).HasMaxLength(255);
        builder.Property(e => e.LastUpdatedByProviderName).HasMaxLength(255);

        builder.Property(e => e.Reference)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.ProviderId).HasColumnType("bigint").IsRequired();
        builder.HasOne(e => e.Provider)
            .WithMany(c=>c.Cohorts)
            .HasForeignKey(c => c.ProviderId)
            .HasPrincipalKey(c=>c.UkPrn);

        builder.Property(e => e.AccountLegalEntityId).HasColumnType("bigint").IsRequired();
        builder.HasOne(c => c.AccountLegalEntity)
            .WithMany(c => c.Cohorts)
            .HasForeignKey(c => c.AccountLegalEntityId)
            .HasPrincipalKey(c => c.Id);

        builder.Property(e => e.Originator).IsRequired().HasColumnType("tinyint");
        builder.Ignore(e => e.DraftApprenticeships);

        builder.HasOne(c => c.TransferSender)
            .WithMany(c => c.TransferFundedCohorts)
            .HasForeignKey(c => c.TransferSenderId);
    }
}