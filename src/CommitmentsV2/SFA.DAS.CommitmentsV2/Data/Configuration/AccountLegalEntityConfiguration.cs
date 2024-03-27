using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SFA.DAS.CommitmentsV2.Models;

namespace SFA.DAS.CommitmentsV2.Data.Configuration;

public class AccountLegalEntityConfiguration : IEntityTypeConfiguration<AccountLegalEntity>
{
    public void Configure(EntityTypeBuilder<AccountLegalEntity> builder)
    {
        builder.Property(ale => ale.Id).ValueGeneratedNever();
        builder.Property(ale => ale.LegalEntityId).IsRequired().HasColumnType("nvarchar(100)");
        builder.Property(ale => ale.PublicHashedId).IsRequired().HasColumnType("nchar(6)");
        builder.Property(ale => ale.Name).IsRequired().HasColumnType("nvarchar(100)");
        builder.Property(ale => ale.OrganisationType).IsRequired().HasConversion(new EnumToNumberConverter< OrganisationType,short>());
        builder.Property(ale => ale.Address).IsRequired().HasColumnType("nvarchar(256)");
        builder.Property(ale => ale.MaLegalEntityId).IsRequired();
        builder.Property(ale => ale.AccountId).IsRequired();
            
        builder.HasOne(ale => ale.Account)
            .WithMany(a => a.AccountLegalEntities)
            .HasPrincipalKey(c=>c.Id).HasForeignKey(c=>c.AccountId);//.Metadata.DeleteBehavior = DeleteBehavior.Restrict;


        builder.HasQueryFilter(ale => ale.Deleted == null);
    }
}