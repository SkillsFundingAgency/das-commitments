using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SFA.DAS.CommitmentsV2.Models;

namespace SFA.DAS.CommitmentsV2.Data.Configuration
{
    public class TransferRequestSummaryConfiguration : IEntityTypeConfiguration<TransferRequestSummary>
    {
        public void Configure(EntityTypeBuilder<TransferRequestSummary> builder)
        {
            builder.HasKey(x => x.CommitmentId);
            builder.Property(x => x.CommitmentId).HasColumnType("long");
            builder.Property(x => x.FundingCap).HasColumnType("money");
            builder.Property(x => x.ApprovedOrRejectedByUserEmail).HasMaxLength(255);
            builder.Property(x => x.ApprovedOrRejectedByUserName).HasMaxLength(255);
            builder.Property(x => x.ApprovedOrRejectedOn).HasColumnType("datetime");
            builder.Property(x => x.CohortReference).HasMaxLength(100);
            builder.Property(x => x.CreatedOn).HasColumnType("datetime");
            builder.Property(x => x.FundingCap).HasColumnType("money");
            builder.Property(x => x.ReceivingEmployerAccountId).HasColumnType("long");
            builder.Property(x => x.SendingEmployerAccountId).HasColumnType("long");
            builder.Property(x => x.Status).HasColumnType("tinyint");
            builder.Property(x => x.TransferCost).HasColumnType("money");
            builder.Property(x => x.TransferRequestId).HasColumnType("long");
        }
    }
}
