using Microsoft.EntityFrameworkCore;
using SFA.DAS.Commitments.EFCoreTester.Data.Models;

namespace SFA.DAS.Commitments.EFCoreTester.Data
{

    public class Constants
    {
        public const string ConnectionString = "Server=(localdb)\\ProjectsV13;Database=SFA.DAS.Commitments.Database;Trusted_Connection=True;";
    }

    public class ProviderDbContext : DbContext
    {
        public DbSet<DraftApprenticeship> DraftApprenticeships { get; set; }
        public DbSet<ConfirmedApprenticeship> ConfirmedApprenticeships { get; set; }
        public virtual DbSet<ApprenticeshipUpdate> ApprenticeshipUpdate { get; set; }
        public virtual DbSet<AssessmentOrganisation> AssessmentOrganisation { get; set; }
        public virtual DbSet<BulkUpload> BulkUpload { get; set; }
        public virtual DbSet<Commitment> Commitment { get; set; }
        public virtual DbSet<CustomProviderPaymentPriority> CustomProviderPaymentPriority { get; set; }
        public virtual DbSet<DataLockStatus> DataLockStatus { get; set; }
        public virtual DbSet<History> History { get; set; }
        public virtual DbSet<IntegrationTestIds> IntegrationTestIds { get; set; }
        public virtual DbSet<JobProgress> JobProgress { get; set; }
        public virtual DbSet<Message> Message { get; set; }
        public virtual DbSet<PriceHistory> PriceHistory { get; set; }
        public virtual DbSet<TransferRequest> TransferRequest { get; set; }


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(Constants.ConnectionString);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("ProductVersion", "2.2.1-servicing-10028");

            modelBuilder.Entity<DraftApprenticeship>().HasBaseType<Apprenticeship>();
            modelBuilder.Entity<ConfirmedApprenticeship>().HasBaseType<Apprenticeship>();

            // modelBuilder.

            modelBuilder.Entity<Apprenticeship>().ToTable("Apprenticeship")
                .HasDiscriminator<short>(nameof(Apprenticeship.PaymentStatus))
                .HasValue<DraftApprenticeship>(0)
                .HasValue<ConfirmedApprenticeship>(1);

            modelBuilder.Entity<ConfirmedApprenticeship>(entity =>
            {
                entity.HasIndex(e => new { e.CommitmentId, e.PaymentStatus, e.AgreedOn })
                    .HasName("IX_Apprenticeship_AgreedOn");

                entity.HasIndex(e => new { e.AgreedOn, e.CommitmentId, e.StartDate, e.StopDate, e.PaymentStatus, e.Uln })
                    .HasName("IX_Apprenticeship_Uln_PaymentStatus");

                entity.HasIndex(e => new { e.AgreedOn, e.AgreementStatus, e.Cost, e.CreatedOn, e.DateOfBirth, e.EmployerRef, e.EndDate, e.FirstName, e.LastName, e.Ninumber, e.PaymentOrder, e.PaymentStatus, e.ProviderRef, e.StartDate, e.TrainingCode, e.TrainingName, e.TrainingType, e.Uln, e.StopDate, e.PauseDate, e.HasHadDataLockSuccess, e.PendingUpdateOriginator, e.CommitmentId })
                    .HasName("IX_Apprenticeship_CommitmentId");

                entity.Property(e => e.AgreedOn).HasColumnType("datetime");

            });

            modelBuilder.Entity<Apprenticeship>(entity =>
            {

                entity.HasIndex(e => new { e.Uln, e.AgreementStatus, e.PaymentStatus })
                    .HasName("IX_Apprenticeship_Uln_Statuses");


                entity.Property(e => e.Cost).HasColumnType("decimal(18, 0)");

                entity.Property(e => e.CreatedOn).HasColumnType("datetime");

                entity.Property(e => e.DateOfBirth).HasColumnType("datetime");

                entity.Property(e => e.EmployerRef).HasMaxLength(50);

                entity.Property(e => e.EndDate).HasColumnType("datetime");

                entity.Property(e => e.EpaorgId)
                    .HasColumnName("EPAOrgId")
                    .HasMaxLength(7)
                    .IsUnicode(false);

                entity.Property(e => e.FirstName).HasMaxLength(100);

                entity.Property(e => e.LastName).HasMaxLength(100);

                entity.Property(e => e.Ninumber)
                    .HasColumnName("NINumber")
                    .HasMaxLength(10);

                entity.Property(e => e.PauseDate).HasColumnType("date");

                entity.Property(e => e.ProviderRef).HasMaxLength(50);

                entity.Property(e => e.StartDate).HasColumnType("datetime");

                entity.Property(e => e.StopDate).HasColumnType("date");

                entity.Property(e => e.TrainingCode).HasMaxLength(20);

                entity.Property(e => e.TrainingName).HasMaxLength(126);

                entity.Property(e => e.Uln)
                    .HasColumnName("ULN")
                    .HasMaxLength(50);

                entity.HasOne(d => d.Commitment)
                    .WithMany(p => p.Apprenticeship)
                    .HasForeignKey(d => d.CommitmentId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Apprenticeship_Commitment");

                entity.HasOne(d => d.Epaorg)
                    .WithMany(p => p.Apprenticeship)
                    .HasPrincipalKey(p => p.EpaorgId)
                    .HasForeignKey(d => d.EpaorgId)
                    .HasConstraintName("FK_Apprenticeship_AssessmentOrganisation");
            });

            modelBuilder.Entity<ApprenticeshipUpdate>(entity =>
            {
                entity.HasIndex(e => new { e.Originator, e.ApprenticeshipId, e.Status })
                    .HasName("IX_ApprenticeshipUpdate_ApprenticeshipId_Status");

                entity.Property(e => e.Cost).HasColumnType("decimal(18, 0)");

                entity.Property(e => e.CreatedOn).HasColumnType("datetime");

                entity.Property(e => e.DateOfBirth).HasColumnType("datetime");

                entity.Property(e => e.EffectiveFromDate).HasColumnType("datetime");

                entity.Property(e => e.EffectiveToDate).HasColumnType("datetime");

                entity.Property(e => e.EndDate).HasColumnType("datetime");

                entity.Property(e => e.FirstName).HasMaxLength(100);

                entity.Property(e => e.LastName).HasMaxLength(100);

                entity.Property(e => e.StartDate).HasColumnType("datetime");

                entity.Property(e => e.TrainingCode).HasMaxLength(20);

                entity.Property(e => e.TrainingName).HasMaxLength(126);
            });

            modelBuilder.Entity<AssessmentOrganisation>(entity =>
            {
                entity.HasIndex(e => e.EpaorgId)
                    .HasName("AK_EPAOrgId")
                    .IsUnique();

                entity.Property(e => e.EpaorgId)
                    .IsRequired()
                    .HasColumnName("EPAOrgId")
                    .HasMaxLength(7)
                    .IsUnicode(false);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(128);
            });

            modelBuilder.Entity<BulkUpload>(entity =>
            {
                entity.Property(e => e.CreatedOn).HasColumnType("datetime");

                entity.Property(e => e.FileContent)
                    .IsRequired()
                    .IsUnicode(false);

                entity.Property(e => e.FileName)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<Commitment>(entity =>
            {
                entity.HasIndex(e => e.TransferSenderId)
                    .HasFilter("([TransferSenderId] IS NOT NULL)");

                entity.HasIndex(e => new { e.EmployerAccountId, e.CommitmentStatus });

                entity.HasIndex(e => new { e.AccountLegalEntityPublicHashedId, e.CreatedOn, e.EditStatus, e.EmployerAccountId, e.LastAction, e.LastUpdatedByEmployerEmail, e.LastUpdatedByEmployerName, e.LastUpdatedByProviderEmail, e.LastUpdatedByProviderName, e.LegalEntityAddress, e.LegalEntityId, e.LegalEntityName, e.LegalEntityOrganisationType, e.ProviderName, e.Reference, e.TransferApprovalActionedByEmployerEmail, e.TransferApprovalActionedByEmployerName, e.TransferApprovalActionedOn, e.TransferApprovalStatus, e.TransferSenderId, e.TransferSenderName, e.ProviderId, e.CommitmentStatus })
                    .HasName("IX_Commitment_ProviderId_CommitmentStatus");

                entity.Property(e => e.AccountLegalEntityPublicHashedId)
                    .HasMaxLength(6)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedOn).HasColumnType("datetime");

                entity.Property(e => e.LastUpdatedByEmployerEmail).HasMaxLength(255);

                entity.Property(e => e.LastUpdatedByEmployerName).HasMaxLength(255);

                entity.Property(e => e.LastUpdatedByProviderEmail).HasMaxLength(255);

                entity.Property(e => e.LastUpdatedByProviderName).HasMaxLength(255);

                entity.Property(e => e.LegalEntityAddress)
                    .IsRequired()
                    .HasMaxLength(256);

                entity.Property(e => e.LegalEntityId)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.LegalEntityName)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.ProviderName).HasMaxLength(100);

                entity.Property(e => e.Reference)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.TransferApprovalActionedByEmployerEmail).HasMaxLength(255);

                entity.Property(e => e.TransferApprovalActionedByEmployerName).HasMaxLength(255);

                entity.Property(e => e.TransferSenderName).HasMaxLength(100);
            });

            modelBuilder.Entity<CustomProviderPaymentPriority>(entity =>
            {
                entity.HasKey(e => new { e.EmployerAccountId, e.ProviderId })
                    .HasName("PK__CustomPr__62EADB9BFA209B9A");
            });

            modelBuilder.Entity<DataLockStatus>(entity =>
            {
                entity.HasIndex(e => e.DataLockEventId);

                entity.HasIndex(e => new { e.ApprenticeshipId, e.PriceEpisodeIdentifier })
                    .HasName("IX_DataLockStatus_ApprenticeshipId")
                    .IsUnique();

                entity.HasIndex(e => new { e.ErrorCode, e.TriageStatus, e.Status, e.IsResolved, e.EventStatus, e.IsExpired, e.IlrEffectiveFromDate, e.Id })
                    .HasName("IX_DataLockStatus_IlrEffectiveFromDate_Id");

                entity.Property(e => e.DataLockEventDatetime).HasColumnType("datetime");

                entity.Property(e => e.EventStatus).HasDefaultValueSql("((1))");

                entity.Property(e => e.Expired).HasColumnType("datetime");

                entity.Property(e => e.IlrActualStartDate).HasColumnType("datetime");

                entity.Property(e => e.IlrEffectiveFromDate).HasColumnType("datetime");

                entity.Property(e => e.IlrPriceEffectiveToDate).HasColumnType("datetime");

                entity.Property(e => e.IlrTotalCost).HasColumnType("decimal(18, 0)");

                entity.Property(e => e.IlrTrainingCourseCode).HasMaxLength(20);

                entity.Property(e => e.PriceEpisodeIdentifier)
                    .IsRequired()
                    .HasMaxLength(25);

                entity.HasOne(d => d.ConfirmedApprenticeship)
                    .WithMany(p => p.DataLockStatus)
                    .HasForeignKey(d => d.ApprenticeshipId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_DataLockStatus_ApprenticeshipId");

                entity.HasOne(d => d.ApprenticeshipUpdate)
                    .WithMany(p => p.DataLockStatus)
                    .HasForeignKey(d => d.ApprenticeshipUpdateId)
                    .HasConstraintName("FK_DataLockStatus_ApprenticeshipUpdateId");
            });

            modelBuilder.Entity<History>(entity =>
            {
                entity.Property(e => e.ChangeType)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.CreatedOn).HasColumnType("datetime");

                entity.Property(e => e.EntityType).HasMaxLength(50);

                entity.Property(e => e.UpdatedByName).HasMaxLength(255);

                entity.Property(e => e.UpdatedByRole)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.UserId)
                    .IsRequired()
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<IntegrationTestIds>(entity =>
            {
                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(256)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<JobProgress>(entity =>
            {
                entity.HasKey(e => e.Lock);

                entity.Property(e => e.Lock)
                    .HasMaxLength(1)
                    .IsUnicode(false)
                    .HasDefaultValueSql("('X')");

                entity.Property(e => e.AddEpaLastSubmissionEventId).HasColumnName("AddEpa_LastSubmissionEventId");

                entity.Property(e => e.IntTestSchemaVersion).HasColumnName("IntTest_SchemaVersion");
            });

            modelBuilder.Entity<Message>(entity =>
            {
                entity.HasIndex(e => new { e.Author, e.CreatedBy, e.CreatedDateTime, e.Text, e.CommitmentId })
                    .HasName("IX_Message_CommitmentId");

                entity.Property(e => e.Author)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(e => e.CreatedDateTime).HasColumnType("datetime");

                entity.Property(e => e.Text)
                    .IsRequired()
                    .HasColumnType("nvarchar(max)");

                entity.HasOne(d => d.Commitment)
                    .WithMany(p => p.Message)
                    .HasForeignKey(d => d.CommitmentId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Message_Commitment");
            });

            modelBuilder.Entity<PriceHistory>(entity =>
            {
                entity.HasIndex(e => new { e.Cost, e.FromDate, e.ToDate, e.ApprenticeshipId })
                    .HasName("IX_PriceHistory_ApprenticeshipId");

                entity.Property(e => e.Cost).HasColumnType("decimal(18, 0)");

                entity.Property(e => e.FromDate).HasColumnType("datetime");

                entity.Property(e => e.ToDate).HasColumnType("datetime");

                entity.HasOne(d => d.ConfirmedApprenticeship)
                    .WithMany(p => p.PriceHistory)
                    .HasForeignKey(d => d.ApprenticeshipId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_PriceHistory_Apprenticeship");
            });

            modelBuilder.Entity<TransferRequest>(entity =>
            {
                entity.HasIndex(e => e.CommitmentId);

                entity.Property(e => e.Cost).HasColumnType("money");

                entity.Property(e => e.CreatedOn).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.FundingCap).HasColumnType("money");

                entity.Property(e => e.TrainingCourses).IsRequired();

                entity.Property(e => e.TransferApprovalActionedByEmployerEmail).HasMaxLength(255);

                entity.Property(e => e.TransferApprovalActionedByEmployerName).HasMaxLength(255);

                entity.HasOne(d => d.Commitment)
                    .WithMany(p => p.TransferRequest)
                    .HasForeignKey(d => d.CommitmentId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_TransferRequest_Commitment");
            });
        }
    }
}
