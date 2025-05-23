﻿using System.Data;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Data.SqlClient;
using SFA.DAS.CommitmentsV2.Configuration;
using SFA.DAS.CommitmentsV2.Data.Configuration;
using SFA.DAS.CommitmentsV2.Models;

namespace SFA.DAS.CommitmentsV2.Data;

public class ProviderCommitmentsDbContext : DbContext, IProviderCommitmentsDbContext
{
    private readonly CommitmentsV2Configuration _configuration;
    private readonly AzureServiceTokenProvider _azureServiceTokenProvider;
    private readonly IDbConnection _connection;
    
    public virtual DbSet<Account> Accounts { get; set; }
    public virtual DbSet<AccountLegalEntity> AccountLegalEntities { get; set; }
    public virtual DbSet<DraftApprenticeship> DraftApprenticeships { get; set; }
    public virtual DbSet<Apprenticeship> Apprenticeships { get; set; }
    public virtual DbSet<ApprenticeshipUpdate> ApprenticeshipUpdates { get; set; }
    public virtual DbSet<AssessmentOrganisation> AssessmentOrganisations { get; set; }
    public virtual DbSet<BulkUpload> BulkUploads { get; set; }
    public virtual DbSet<Cohort> Cohorts { get; set; }
    public virtual DbSet<CustomProviderPaymentPriority> CustomProviderPaymentPriorities { get; set; }
    public virtual DbSet<DataLockStatus> DataLocks { get; set; }
    public virtual DbSet<DataLockUpdaterJobStatus> DataLockUpdaterJobStatuses { get; set; }
    public virtual DbSet<DataLockUpdaterJobHistory> DataLockUpdaterJobHistory { get; set; }
    public virtual DbSet<History> History { get; set; }
    public virtual DbSet<IntegrationTestIds> IntegrationTestIds { get; set; }
    public virtual DbSet<JobProgress> JobProgress { get; set; }
    public virtual DbSet<Message> Messages { get; set; }
    public virtual DbSet<Provider> Providers { get; set; }
    public virtual DbSet<PriceHistory> PriceHistory { get; set; }
    public virtual DbSet<TransferRequest> TransferRequests { get; set; }
    public virtual DbSet<ChangeOfPartyRequest> ChangeOfPartyRequests { get; set; }
    public virtual DbSet<Standard> Standards { get; set; }
    public virtual DbSet<StandardFundingPeriod> StandardFundingPeriods { get; set; }
    public virtual DbSet<StandardOption> StandardOptions { get; set; }
    public virtual DbSet<Framework> Frameworks { get; set; }
    public virtual DbSet<FrameworkFundingPeriod> FrameworkFundingPeriods { get; set; }
    public virtual DbSet<ApprenticeshipConfirmationStatus> ApprenticeshipConfirmationStatus { get; set; }
    public virtual DbSet<OverlappingEmail> OverlappingEmails { get; set; }
    public virtual DbSet<Learner> Learners { get; set; }
    public virtual DbSet<OverlappingTrainingDateRequest> OverlappingTrainingDateRequests { get; set; }
    public virtual DbSet<FileUploadLog> FileUploadLogs { get; set; }

    public ProviderCommitmentsDbContext(DbContextOptions<ProviderCommitmentsDbContext> options) : base(options)
    {
    }
        
    public ProviderCommitmentsDbContext(
        IDbConnection connection,
        CommitmentsV2Configuration configuration,
        AzureServiceTokenProvider azureServiceTokenProvider,
        DbContextOptions<ProviderCommitmentsDbContext> options) : base(options)
    {
        _configuration = configuration;
        _azureServiceTokenProvider = azureServiceTokenProvider;
        _connection = connection;
    }

    protected ProviderCommitmentsDbContext()
    {
    }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (_configuration == null || _azureServiceTokenProvider == null)
        {
            return;
        }

        optionsBuilder.UseSqlServer(_connection as SqlConnection);
    }

    public virtual Task ExecuteSqlCommandAsync(string sql, params object[] parameters)
    {
        return Database.ExecuteSqlRawAsync(sql, parameters);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new AccountConfiguration());
        modelBuilder.ApplyConfiguration(new AccountLegalEntityConfiguration());
        modelBuilder.ApplyConfiguration(new ApprenticeshipConfiguration());
        modelBuilder.ApplyConfiguration(new ApprenticeshipBaseConfiguration());
        modelBuilder.ApplyConfiguration(new ApprenticeshipUpdateConfiguration());
        modelBuilder.ApplyConfiguration(new ApprenticeshipConfirmationStatusConfiguration());
        modelBuilder.ApplyConfiguration(new OverlappingEmailConfiguration());
        modelBuilder.ApplyConfiguration(new AssessmentOrganisationConfiguration());
        modelBuilder.ApplyConfiguration(new BulkUploadConfiguration());
        modelBuilder.ApplyConfiguration(new CohortConfiguration());
        
        modelBuilder.ApplyConfiguration(new CustomProviderPaymentPriorityConfiguration());
        modelBuilder.ApplyConfiguration(new DataLockStatusConfiguration());
        modelBuilder.ApplyConfiguration(new DataLockUpdaterJobStatusConfiguration());
        modelBuilder.ApplyConfiguration(new DraftApprenticeshipConfiguration());
        modelBuilder.ApplyConfiguration(new HistoryConfiguration());
        modelBuilder.ApplyConfiguration(new JobProgressConfiguration());
        modelBuilder.ApplyConfiguration(new MessageConfiguration());
        modelBuilder.ApplyConfiguration(new PriceHistoryConfiguration());
        modelBuilder.ApplyConfiguration(new ProviderConfiguration());
        modelBuilder.ApplyConfiguration(new TransferRequestConfiguration());
        modelBuilder.ApplyConfiguration(new ChangeOfPartyRequestConfiguration());
        modelBuilder.ApplyConfiguration(new StandardConfiguration());
        modelBuilder.ApplyConfiguration(new StandardOptionConfiguration());
        modelBuilder.ApplyConfiguration(new FrameworkConfiguration());
        modelBuilder.ApplyConfiguration(new FrameworkFundingPeriodConfiguration());
        modelBuilder.ApplyConfiguration(new StandardFundingPeriodConfiguration());
        modelBuilder.ApplyConfiguration(new LearnerConfiguration());           
        modelBuilder.ApplyConfiguration(new FlexibleEmploymentConfiguration());           
        modelBuilder.ApplyConfiguration(new ApprenticeshipPriorLearningConfiguration());
        modelBuilder.ApplyConfiguration(new OverlappingTrainingDateRequestConfiguration());
        modelBuilder.ApplyConfiguration(new FileUploadLogConfiguration());
    }
        
    public override void Dispose()
    {
        _connection?.Dispose();
        
        base.Dispose();
    }
}