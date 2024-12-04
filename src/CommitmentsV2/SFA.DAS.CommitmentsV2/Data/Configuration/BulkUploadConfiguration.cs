using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SFA.DAS.CommitmentsV2.Models;

namespace SFA.DAS.CommitmentsV2.Data.Configuration;

public class BulkUploadConfiguration : IEntityTypeConfiguration<BulkUpload>
{
    public void Configure(EntityTypeBuilder<BulkUpload> builder)
    {
        builder.Property(e => e.CreatedOn).HasColumnType("datetime");

        builder.Property(e => e.FileContent)
            .IsRequired()
            .IsUnicode(false);

        builder.Property(e => e.FileName)
            .IsRequired()
            .HasMaxLength(50)
            .IsUnicode(false);
    }
}