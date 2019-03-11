using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SFA.DAS.CommitmentsV2.Models;

namespace SFA.DAS.CommitmentsV2.Data.Configuration
{
    public class CustomProviderPaymentPriorityConfiguration : IEntityTypeConfiguration<CustomProviderPaymentPriority>
    {
        public void Configure(EntityTypeBuilder<CustomProviderPaymentPriority> builder)
        {
            // No special customization required for this class
        }
    }
}