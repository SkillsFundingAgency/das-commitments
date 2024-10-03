using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SFA.DAS.CommitmentsV2.Models;

namespace SFA.DAS.CommitmentsV2.Data.Configuration;

public class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.Property(a => a.Id).ValueGeneratedNever();
        builder.Property(a => a.HashedId).IsRequired().HasColumnType("nchar(6)");
        builder.Property(a => a.PublicHashedId).IsRequired().HasColumnType("nchar(6)");
        builder.Property(a => a.Name).IsRequired().HasColumnType("nvarchar(100)");

        builder.HasMany(c => c.CustomProviderPaymentPriorities)
            .WithOne(c => c.EmployerAccount)
            .HasForeignKey(c => c.EmployerAccountId);
    }
}