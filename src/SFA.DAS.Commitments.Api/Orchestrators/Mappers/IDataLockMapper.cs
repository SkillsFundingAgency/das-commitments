using SFA.DAS.Commitments.Api.Types.Apprenticeship.Types;
using SFA.DAS.Commitments.Api.Types.DataLock;

namespace SFA.DAS.Commitments.Api.Orchestrators.Mappers
{
    public interface IDataLockMapper
    {
        DataLockStatus Map(Domain.Entities.DataLock.DataLockStatus domainDataLock);
    }

    public class DataLockMapper : IDataLockMapper
    {
        public DataLockStatus Map(Domain.Entities.DataLock.DataLockStatus domainDataLock)
        {
         
            return new DataLockStatus
            {
                ApprenticeshipId = domainDataLock.ApprenticeshipId,
                DataLockEventDatetime = domainDataLock.DataLockEventDatetime,
                DataLockEventId = domainDataLock.DataLockEventId,
                ErrorCode = (Api.Types.DataLock.Types.DataLockErrorCode)domainDataLock.ErrorCode,
                IlrActualStartDate = domainDataLock.IlrActualStartDate,
                IlrEffectiveFromDate = domainDataLock.IlrEffectiveFromDate,
                IlrTotalCost = domainDataLock.IlrTotalCost,
                IlrTrainingCourseCode = domainDataLock.IlrTrainingCourseCode,
                IlrTrainingType = (TrainingType)domainDataLock.IlrTrainingType,
                PriceEpisodeIdentifier = domainDataLock.PriceEpisodeIdentifier,
                Status = (Api.Types.DataLock.Types.Status)domainDataLock.Status,
                TriageStatus = (Api.Types.DataLock.Types.TriageStatus)domainDataLock.TriageStatus,
                ApprenticeshipUpdateId = domainDataLock.ApprenticeshipUpdateId,
                IsResolved = domainDataLock.IsResolved
            };
        }
    }
}