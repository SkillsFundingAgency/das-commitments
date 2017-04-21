using System.Linq;

using SFA.DAS.Commitments.Domain.Entities.DataLock;
using SFA.DAS.Provider.Events.Api.Types;

using DataLockEventError = SFA.DAS.Commitments.Domain.Entities.DataLock.DataLockEventError;

namespace SFA.DAS.Commitments.Infrastructure.Services
{
    public interface IPaymentEventMapper
    {
        DataLockEventItem Map(DataLockEvent result);
    }

    public class PaymentEventMapper : IPaymentEventMapper
    {
        public DataLockEventItem Map(DataLockEvent result)
        {
            return new DataLockEventItem
                       {
                           DataLockEventId = result.Id,
                           DataLockEventDatetime = result.ProcessDateTime,
                           PriceEpisodeIdentifier = result.PriceEpisodeIdentifier,
                           ApprenticeshipId =  result.ApprenticeshipId,
                           IlrActualStartDate = result.IlrStartDate,
                           IlrEffectiveFromDate = result.IlrPriceEffectiveDate,
                           IlrTotalCost = result.IlrTrainingPrice + result.IlrEndpointAssessorPrice,
                           ErrorCodes = result.Errors.Select(MapCodes),
                           DataLockStatus = result.Errors.Any() ? DataLockStatus.Fail : DataLockStatus.Pass
                       };
        }

        private DataLockEventError MapCodes(Provider.Events.Api.Types.DataLockEventError error)
        {
            return new DataLockEventError
                       {
                           ErrorCode = error.ErrorCode,
                           SystemDescription = error.SystemDescription
                       };
        }
    }
}
