using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SFA.DAS.CommitmentsV2.Models;

namespace SFA.DAS.CommitmentsV2.Data
{
    public class AccountConfiguration : IEntityTypeConfiguration<Account>
    {
        public void Configure(EntityTypeBuilder<Account> builder)
        {
            builder.Property(a => a.Id).ValueGeneratedNever();
            builder.Property(a => a.HashedId).IsRequired().HasColumnType("char(6)");
            builder.Property(a => a.PublicHashedId).IsRequired().HasColumnType("char(6)");
            builder.Property(a => a.Name).IsRequired().HasColumnType("nvarchar(100)");
        }
    }
}