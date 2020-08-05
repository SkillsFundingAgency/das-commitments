using System.Collections.Generic;
using System.Threading.Tasks;
using SFA.DAS.Commitments.Domain.Entities;
using System;

namespace SFA.DAS.Commitments.Domain.Data
{
    public interface IApprenticeshipRepository
    {
        Task UpdateApprenticeship(Apprenticeship apprenticeship, Caller caller);

        Task StopApprenticeship(long commitmentId, long apprenticeshipId, DateTime dateOfChange, bool? madeRedundant);

        Task ResumeApprenticeship(long commitmentId, long apprenticeshipId);

        Task PauseApprenticeship(long commitmentId, long apprenticeshipId, DateTime pauseDate);

        Task UpdateApprenticeshipEpa(long apprenticeshipId, string epaOrgId);

        Task UpdateApprenticeshipStatus(long commitmentId, long apprenticeshipId, PaymentStatus paymentStatus);

        Task DeleteApprenticeship(long apprenticeshipId);

        Task<IList<Apprenticeship>> BulkUploadApprenticeships(long commitmentId, IEnumerable<Apprenticeship> apprenticeships);

        Task<ApprenticeshipsResult> GetApprenticeshipsByEmployer(long accountId, string searchKeyword = "");

        Task<Apprenticeship> GetApprenticeship(long apprenticeshipId);

        Task<IEnumerable<ApprenticeshipResult>> GetActiveApprenticeshipsByUlns(IEnumerable<string> ulns);

        Task<IEnumerable<ApprenticeshipStatusSummary>> GetApprenticeshipSummariesByEmployer(long employerAccountId);

        Task InsertPriceHistory(long apprenticeshipId, IEnumerable<PriceHistory> priceHistory);

        Task<IEnumerable<PriceHistory>> GetPriceHistory(long apprenticeshipId);

        Task CreatePriceHistoryForApprenticeshipsInCommitment(long commitmentId);

        Task<IList<AlertSummary>> GetEmployerApprenticeshipAlertSummary();

        Task<IList<ProviderAlertSummary>> GetProviderApprenticeshipAlertSummary();
        
        Task SetHasHadDataLockSuccess(long id);
        Task UpdateApprenticeshipStopDate(long commitmentId, long apprenticeshipId, DateTime stopDate);

        Task<ApprenticeshipsResult> GetApprenticeshipsByUln(string uln, long accountId);

        Task<IEnumerable<long>> GetEmployerAccountIds();
        Task<ApprenticeshipsResult> GetApprovedApprenticeshipsByProvider(long providerId);
        Task<ApprenticeshipsResult> GetApprovedApprenticeshipsByEmployer(long accountId);
    }
}