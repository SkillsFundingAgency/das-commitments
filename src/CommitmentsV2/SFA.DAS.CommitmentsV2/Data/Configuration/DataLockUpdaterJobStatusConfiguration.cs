using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SFA.DAS.CommitmentsV2.Models;

namespace SFA.DAS.CommitmentsV2.Data.Configuration;

public class DataLockUpdaterJobStatusConfiguration : IEntityTypeConfiguration<DataLockUpdaterJobStatus>
{
    public void Configure(EntityTypeBuilder<DataLockUpdaterJobStatus> builder)
    {
        builder.ToTable("DataLockUpdaterJobStatus");
        builder.HasKey(p => p.Id).IsClustered();
    }
}