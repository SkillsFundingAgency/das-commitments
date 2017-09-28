using SFA.DAS.Commitments.Api.Types.Apprenticeship.Types;
using SFA.DAS.Commitments.Api.Types.DataLock;
using SFA.DAS.Commitments.Api.Types.DataLock.Types;

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
                ErrorCode = (DataLockErrorCode)domainDataLock.ErrorCode,
                IlrActualStartDate = domainDataLock.IlrActualStartDate,
                IlrEffectiveFromDate = domainDataLock.IlrEffectiveFromDate,
                IlrEffectiveToDate = domainDataLock.IlrPriceEffectiveToDate,
                IlrTotalCost = domainDataLock.IlrTotalCost,
                IlrTrainingCourseCode = domainDataLock.IlrTrainingCourseCode,
                IlrTrainingType = (TrainingType)domainDataLock.IlrTrainingType,
                PriceEpisodeIdentifier = domainDataLock.PriceEpisodeIdentifier,
                Status = (Status)domainDataLock.Status,
                TriageStatus = (TriageStatus)domainDataLock.TriageStatus,
                ApprenticeshipUpdateId = domainDataLock.ApprenticeshipUpdateId,
                IsResolved = domainDataLock.IsResolved,
                EventStatus = domainDataLock.EventStatus == Domain.Entities.EventStatus.None 
                    ? EventStatus.New 
                    : (EventStatus)domainDataLock.EventStatus
            };
        }
    }
}