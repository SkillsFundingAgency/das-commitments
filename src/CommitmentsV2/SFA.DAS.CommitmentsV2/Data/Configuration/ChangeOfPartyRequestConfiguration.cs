using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SFA.DAS.CommitmentsV2.Models;

namespace SFA.DAS.CommitmentsV2.Data.Configuration
{
    public class ChangeOfPartyRequestConfiguration : IEntityTypeConfiguration<ChangeOfPartyRequest>
    {
        public void Configure(EntityTypeBuilder<ChangeOfPartyRequest> builder)
        {
            builder.Property(p => p.RowVersion).IsConcurrencyToken().ValueGeneratedOnAddOrUpdate();
        }
    }
}
