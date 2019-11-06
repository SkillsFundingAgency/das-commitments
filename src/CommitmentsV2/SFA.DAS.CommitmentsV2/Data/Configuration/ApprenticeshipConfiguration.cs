﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SFA.DAS.CommitmentsV2.Models;

namespace SFA.DAS.CommitmentsV2.Data.Configuration
{
    public class ApprenticeshipConfiguration : IEntityTypeConfiguration<Apprenticeship>
    {
        public void Configure(EntityTypeBuilder<Apprenticeship> builder)
        {
            SetTablePerHierarchy(builder);
            
            builder.Property(e => e.Cost).HasColumnType("decimal(18, 0)");
            builder.Property(e => e.CreatedOn).HasColumnType("datetime");
            builder.Property(e => e.DateOfBirth).HasColumnType("datetime");
            builder.Property(e => e.EmployerRef).HasMaxLength(50);
            builder.Property(e => e.EndDate).HasColumnType("datetime");

            builder.Property(e => e.EpaOrgId)
                .HasColumnName("EPAOrgId")
                .HasMaxLength(7)
                .IsUnicode(false);

            builder.Property(e => e.FirstName).HasMaxLength(100);
            builder.Property(e => e.LastName).HasMaxLength(100);

            builder.Property(e => e.NiNumber)
                .HasColumnName("NINumber")
                .HasMaxLength(10);

            builder.Property(e => e.PauseDate).HasColumnType("date");
            builder.Property(e => e.ProviderRef).HasMaxLength(50);
            builder.Property(e => e.StartDate).HasColumnType("datetime");
            builder.Property(e => e.StopDate).HasColumnType("date");

            builder.Property(e => e.ProgrammeType)
                .HasColumnName("TrainingType");

            builder.Property(e => e.CourseCode)
                .HasColumnName("TrainingCode")
                .HasMaxLength(20);

            builder.Property(e => e.ProgrammeType).HasColumnName("TrainingType");

            builder.Property(e => e.CourseName)
                .HasColumnName("TrainingName")
                .HasMaxLength(126);

            builder.Property(e => e.Uln)
                .HasColumnName("Uln")
                .HasMaxLength(50);

            builder.HasOne(d => d.Cohort)
                .WithMany(p => p.Apprenticeships)
                .HasForeignKey(d => d.CommitmentId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(d => d.EpaOrg)
                .WithMany(p => p.Apprenticeship)
                .HasPrincipalKey(p => p.EpaOrgId)
                .HasForeignKey(d => d.EpaOrgId);
            
            builder.Property(e => e.ProgrammeType).HasColumnName("TrainingType");
        }

        private void SetTablePerHierarchy(EntityTypeBuilder<Apprenticeship> builder)
        {
            /*
             *  TPH requires a discriminator column. By default this is called Discriminator and is a string, but this can be configured.
             *  Here, the discriminator column is set to "IsApproved" and is a boolean.
             *  We cannot use PaymentStatus directly because the discriminator requires one value for each entity type which
             *  doesn't match the scenario since paymentstatus 0 means Draft and *all* other values mean approved.
             *  So we create a calculated field in the database called IsApproved which is based on payment status:
             * alter table [dbo].[Apprenticeship]
             *      add IsApproved as (CASE WHEN PaymentStatus > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END) PERSISTED;
.            * Note that the value is persisted, since we will be selecting on this column.
             * The fact that this is calculated field means that EF does not attempt to set it (which it would normally
             * - do based on the discriminator for that entity type).
             */

            builder.ToTable("Apprenticeship")
                .HasDiscriminator<bool>(nameof(Apprenticeship.IsApproved))
                .HasValue<DraftApprenticeship>(false)
                .HasValue<ApprovedApprenticeship>(true);

            builder.Property(p => p.IsApproved)
                .HasComputedColumnSql("CASE WHEN PaymentStatus > 0 THEN 1 ELSE 0 END");
        }
    }
}