using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SFA.DAS.CommitmentsV2.Models;

namespace SFA.DAS.CommitmentsV2.Data.Configuration
{
    public class FileUploadLogConfiguration : IEntityTypeConfiguration<FileUploadLog>
    {
        public void Configure(EntityTypeBuilder<FileUploadLog> builder)
        {
            builder.ToTable("FileUploadLog")
                .HasKey("Id");

            builder.HasMany(d => d.CohortLogs)
                .WithOne(x => x.FileUploadLog)
                .HasForeignKey(x => x.FileUploadLogId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}