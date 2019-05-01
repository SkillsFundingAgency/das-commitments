using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SFA.DAS.CommitmentsV2.Models;

namespace SFA.DAS.CommitmentsV2.Data.Configuration
{
    public class MessageConfiguration : IEntityTypeConfiguration<Message>
    {
        public void Configure(EntityTypeBuilder<Message> builder)
        {
            builder.Property(e => e.Author)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(e => e.CreatedDateTime).HasColumnType("datetime");

            builder.Property(e => e.Text)
                .IsRequired();

            builder.HasOne(d => d.Cohort)
                .WithMany(p => p.Message)
                .HasForeignKey(d => d.CommitmentId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        }
    }
}