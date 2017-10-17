﻿using System.Collections.Generic;
using System.Threading.Tasks;
using SFA.DAS.Commitments.Domain.Entities;
using System;

namespace SFA.DAS.Commitments.Domain.Data
{
    public interface IApprenticeshipRepository
    {
        Task<long> CreateApprenticeship(Apprenticeship apprenticeship);

        Task UpdateApprenticeship(Apprenticeship apprenticeship, Caller caller);

        Task StopApprenticeship(long commitmentId, long apprenticeshipId, DateTime dateOfChange);

        Task ResumeApprenticeship(long commitmentId, long apprenticeshipId);

        Task PauseApprenticeship(long commitmentId, long apprenticeshipId, DateTime pauseDate);

        Task UpdateApprenticeshipStatus(long commitmentId, long apprenticeshipId, PaymentStatus paymentStatus);

        Task UpdateApprenticeshipStatus(long commitmentId, long apprenticeshipId, AgreementStatus agreementStatus);

        Task UpdateApprenticeshipStatuses(List<Apprenticeship> apprenticeships);

        Task DeleteApprenticeship(long apprenticeshipId);

        Task<IList<Apprenticeship>> BulkUploadApprenticeships(long commitmentId, IEnumerable<Apprenticeship> apprenticeships);

        Task<ApprenticeshipsResult> GetApprenticeshipsByProvider(long providerId, string searchKeyword = "");

        Task<ApprenticeshipsResult> GetApprenticeshipsByEmployer(long accountId, string searchKeyword = "");

        Task<Apprenticeship> GetApprenticeship(long apprenticeshipId);

        Task<IList<ApprenticeshipResult>> GetActiveApprenticeshipsByUlns(IEnumerable<string> ulns);

        Task<IEnumerable<ApprenticeshipStatusSummary>> GetApprenticeshipSummariesByEmployer(long employerAccountId);

        Task InsertPriceHistory(long apprenticeshipId, IEnumerable<PriceHistory> priceHistory);

        Task<IEnumerable<PriceHistory>> GetPriceHistory(long apprenticeshipId);

        Task CreatePriceHistoryForApprenticeshipsInCommitment(long commitmentId);

        Task<IList<AlertSummary>> GetEmployerApprenticeshipAlertSummary();

        Task<IList<ProviderAlertSummary>> GetProviderApprenticeshipAlertSummary();
        
        Task SetHasHadDataLockSuccess(long id);
    }
}