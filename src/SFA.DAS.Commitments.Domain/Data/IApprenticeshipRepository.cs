using System.Collections.Generic;
using System.Threading.Tasks;

using SFA.DAS.Commitments.Domain.Entities;
using System;

namespace SFA.DAS.Commitments.Domain.Data
{
    public interface IApprenticeshipRepository
    {
        Task<long> CreateApprenticeship(Apprenticeship apprenticeship, CallerType callerType, string userId);

        Task UpdateApprenticeship(Apprenticeship apprenticeship, Caller caller, string userId);

        Task StopApprenticeship(long commitmentId, long apprenticeshipId, DateTime dateOfChange);

        Task UpdateApprenticeshipStatus(long commitmentId, long apprenticeshipId, PaymentStatus paymentStatus);

        Task UpdateApprenticeshipStatus(long commitmentId, long apprenticeshipId, AgreementStatus agreementStatus);

        Task DeleteApprenticeship(long apprenticeshipId, CallerType callerType, string userId, long commitmentId);

        Task<IList<Apprenticeship>> BulkUploadApprenticeships(long commitmentId, IEnumerable<Apprenticeship> apprenticeships, CallerType caller, string userId);

        Task<IList<Apprenticeship>> GetApprenticeshipsByProvider(long providerId);

        Task<IList<Apprenticeship>> GetApprenticeshipsByEmployer(long accountId);

        Task<Apprenticeship> GetApprenticeship(long apprenticeshipId);

        Task<IList<ApprenticeshipResult>> GetActiveApprenticeshipsByUlns(IEnumerable<string> ulns);
    }
}