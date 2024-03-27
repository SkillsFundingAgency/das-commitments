using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SFA.DAS.CommitmentsV2.Models;

namespace SFA.DAS.CommitmentsV2.Data.Configuration
{
    public class ApprenticeshipUpdateConfiguration : IEntityTypeConfiguration<ApprenticeshipUpdate>
    {
        public void Configure(EntityTypeBuilder<ApprenticeshipUpdate> builder)
        {
            builder.ToTable("ApprenticeshipUpdate").HasKey(e => e.Id);
            builder.Property(e => e.Cost).HasColumnType("decimal(18, 0)");
            builder.Property(e => e.CreatedOn).HasColumnType("datetime");
            builder.Property(e => e.DateOfBirth).HasColumnType("datetime");
            builder.Property(e => e.EffectiveFromDate).HasColumnType("datetime");
            builder.Property(e => e.EffectiveToDate).HasColumnType("datetime");
            builder.Property(e => e.EndDate).HasColumnType("datetime");
            builder.Property(e => e.FirstName).HasMaxLength(100);
            builder.Property(e => e.LastName).HasMaxLength(100);
            builder.Property(e => e.Email).HasMaxLength(200);
            builder.Property(e => e.StartDate).HasColumnType("datetime");
            builder.Property(e => e.DeliveryModel).HasColumnType("tinyint");
            builder.Property(e => e.TrainingCode).HasMaxLength(20);
            builder.Property(e => e.TrainingCourseVersion).HasMaxLength(5);
            builder.Property(e => e.TrainingCourseVersionConfirmed).HasColumnType("bool");
            builder.Property(e => e.TrainingName).HasMaxLength(126);
            builder.Property(e => e.TrainingCourseOption).HasMaxLength(200);
            builder.Property(e => e.StandardUId).HasMaxLength(20);
            builder.Property(e => e.Originator).IsRequired().HasColumnType("tinyint");
            builder.Property(e => e.Status).IsRequired().HasColumnType("tinyint");
            builder.Property(e => e.UpdateOrigin).HasColumnType("tinyint");
        }
    }
}