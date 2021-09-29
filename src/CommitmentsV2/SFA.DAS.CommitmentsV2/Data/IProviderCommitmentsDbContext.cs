using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SFA.DAS.CommitmentsV2.Models;

namespace SFA.DAS.CommitmentsV2.Data
{
    public interface IProviderCommitmentsDbContext : ICommitmentsDbContext
    {

    }

    public interface ICommitmentsDbContext
    {
        DbSet<Account> Accounts { get; set; }
        DbSet<AccountLegalEntity> AccountLegalEntities { get; set; }
        DbSet<DraftApprenticeship> DraftApprenticeships { get; set; }
        DbSet<Apprenticeship> Apprenticeships { get; set; }
        DbSet<ApprenticeshipUpdate> ApprenticeshipUpdates { get; set; }
        DbSet<AssessmentOrganisation> AssessmentOrganisations { get; set; }
        DbSet<BulkUpload> BulkUploads { get; set; }
        DbSet<Cohort> Cohorts { get; set; }
        DbSet<CustomProviderPaymentPriority> CustomProviderPaymentPriorities { get; set; }
        DbSet<DataLockStatus> DataLocks { get; set; }
        DbSet<History> History { get; set; }
        DbSet<IntegrationTestIds> IntegrationTestIds { get; set; }
        DbSet<JobProgress> JobProgress { get; set; }
        DbSet<Message> Messages { get; set; }
        DbSet<Provider> Providers { get; set; }
        DbSet<PriceHistory> PriceHistory { get; set; }
        DbSet<TransferRequest> TransferRequests { get; set; }
        DbSet<Standard> Standards { get; set; }
        DbSet<StandardOption> StandardOptions { get; set; }
        DbSet<Framework> Frameworks { get; set; }
        Task ExecuteSqlCommandAsync(string sql, params object[] parameters);
    }
}