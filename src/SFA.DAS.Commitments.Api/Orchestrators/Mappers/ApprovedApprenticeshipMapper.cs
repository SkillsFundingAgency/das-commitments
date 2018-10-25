using SFA.DAS.Commitments.Api.Types.Apprenticeship.Types;
using SFA.DAS.Commitments.Api.Types.ApprovedApprenticeship;

namespace SFA.DAS.Commitments.Api.Orchestrators.Mappers
{
    public class ApprovedApprenticeshipMapper : IApprovedApprenticeshipMapper
    {
        private readonly IDataLockMapper _dataLockMapper;
        private readonly IApprenticeshipMapper _apprenticeshipMapper;

        public ApprovedApprenticeshipMapper(IDataLockMapper dataLockMapper, IApprenticeshipMapper apprenticeshipMapper)
        {
            //todo: split apprenticeship update and price history mapping into their own mappers and inject those instead
            _dataLockMapper = dataLockMapper;
            _apprenticeshipMapper = apprenticeshipMapper;
        }

        public ApprovedApprenticeship Map(Domain.Entities.ApprovedApprenticeship.ApprovedApprenticeship domainObject)
        {
            var result = new ApprovedApprenticeship
            {
                Id = domainObject.Id,
                CohortReference = domainObject.CohortReference,
                EmployerAccountId = domainObject.EmployerAccountId,
                ProviderId = domainObject.ProviderId,
                TransferSenderId = domainObject.TransferSenderId,
                Reference = domainObject.Reference,
                FirstName = domainObject.FirstName,
                LastName = domainObject.LastName,
                DateOfBirth = domainObject.DateOfBirth,
                ULN = domainObject.ULN,
                TrainingType = (TrainingType) domainObject.TrainingType,
                TrainingCode = domainObject.TrainingCode,
                TrainingName = domainObject.TrainingName,
                StartDate = domainObject.StartDate,
                EndDate = domainObject.EndDate,
                PauseDate = domainObject.PauseDate,
                StopDate = domainObject.StopDate,
                PaymentStatus = (PaymentStatus) domainObject.PaymentStatus,
                EmployerRef = domainObject.EmployerRef,
                ProviderRef = domainObject.ProviderRef,
                PaymentOrder = domainObject.PaymentOrder,
                UpdateOriginator = domainObject.UpdateOriginator == null ? default(Originator?) : (Originator) domainObject.UpdateOriginator,
                ProviderName = domainObject.ProviderName,
                LegalEntityId = domainObject.LegalEntityId,
                LegalEntityName = domainObject.LegalEntityName,
                AccountLegalEntityPublicHashedId = domainObject.AccountLegalEntityPublicHashedId,
                HasHadDataLockSuccess = domainObject.HasHadDataLockSuccess,
                EndpointAssessorName = domainObject.EndpointAssessorName
            };

            //data locks
            foreach (var domainDataLock in domainObject.DataLocks)
            {
                result.DataLocks.Add(_dataLockMapper.Map(domainDataLock));
            }

            //price episodes
            foreach (var domainPriceEpisode in domainObject.PriceEpisodes)
            {
                result.PriceEpisodes.Add(_apprenticeshipMapper.MapPriceHistory(domainPriceEpisode));
            }

            //pending update
            if (domainObject.PendingUpdate != null)
            {
                result.PendingUpdate = _apprenticeshipMapper.MapApprenticeshipUpdate(domainObject.PendingUpdate);
            }

            return result;
        }
    }
}