using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SFA.DAS.CommitmentsV2.Data.Configuration;
using SFA.DAS.CommitmentsV2.Models;

namespace SFA.DAS.CommitmentsV2.Data
{
    public class CommitmentsReadOnlyDbContext : DbContext, ICommitmentsReadOnlyDbContext
    {
        protected CommitmentsReadOnlyDbContext()
        {
        }
        public CommitmentsReadOnlyDbContext(DbContextOptions<CommitmentsReadOnlyDbContext> options) : base(options)
        {
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            
        }
        public override int SaveChanges()
        {
            throw new InvalidOperationException("Read only context");
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            throw new InvalidOperationException("Read only context");
        }

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = new CancellationToken())
        {
            throw new InvalidOperationException("Read only context");
        }

        public DbSet<Account> Accounts { get; set; }
        public DbSet<AccountLegalEntity> AccountLegalEntities { get; set; }
        public DbSet<DraftApprenticeship> DraftApprenticeships { get; set; }
        public DbSet<Apprenticeship> Apprenticeships { get; set; }
        public DbSet<ApprenticeshipUpdate> ApprenticeshipUpdates { get; set; }
        public DbSet<AssessmentOrganisation> AssessmentOrganisations { get; set; }
        public DbSet<BulkUpload> BulkUploads { get; set; }
        public DbSet<Cohort> Cohorts { get; set; }
        public DbSet<CustomProviderPaymentPriority> CustomProviderPaymentPriorities { get; set; }
        public DbSet<DataLockStatus> DataLocks { get; set; }
        public DbSet<History> History { get; set; }
        public DbSet<IntegrationTestIds> IntegrationTestIds { get; set; }
        public DbSet<JobProgress> JobProgress { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Provider> Providers { get; set; }
        public DbSet<PriceHistory> PriceHistory { get; set; }
        public DbSet<TransferRequest> TransferRequests { get; set; }
        public Task ExecuteSqlCommandAsync(string sql, params object[] parameters)
        {
            return Database.ExecuteSqlCommandAsync(sql, parameters);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new AccountConfiguration());
            modelBuilder.ApplyConfiguration(new AccountLegalEntityConfiguration());
            modelBuilder.ApplyConfiguration(new ApprenticeshipBaseConfiguration());
            modelBuilder.ApplyConfiguration(new ApprenticeshipUpdateConfiguration());
            modelBuilder.ApplyConfiguration(new AssessmentOrganisationConfiguration());
            modelBuilder.ApplyConfiguration(new BulkUploadConfiguration());
            modelBuilder.ApplyConfiguration(new CohortConfiguration());
            modelBuilder.ApplyConfiguration(new ApprenticeshipConfiguration());
            modelBuilder.ApplyConfiguration(new CustomProviderPaymentPriorityConfiguration());
            modelBuilder.ApplyConfiguration(new DataLockStatusConfiguration());
            modelBuilder.ApplyConfiguration(new DraftApprenticeshipConfiguration());
            modelBuilder.ApplyConfiguration(new HistoryConfiguration());
            modelBuilder.ApplyConfiguration(new JobProgressConfiguration());
            modelBuilder.ApplyConfiguration(new MessageConfiguration());
            modelBuilder.ApplyConfiguration(new PriceHistoryConfiguration());
            modelBuilder.ApplyConfiguration(new ProviderConfiguration());
            modelBuilder.ApplyConfiguration(new TransferRequestConfiguration());
        }
    }
}