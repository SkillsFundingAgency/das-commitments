using System;
using System.Collections.Generic;
using System.Linq;

using SFA.DAS.Commitments.Domain.Entities.DataLock;
using SFA.DAS.Provider.Events.Api.Types;

namespace SFA.DAS.Commitments.Infrastructure.Services
{
    public class PaymentEventMapper : IPaymentEventMapper
    {
        public DataLockStatus Map(DataLockEvent result)
        {
            return new DataLockStatus
                       {
                           DataLockEventId = result.Id,
                           DataLockEventDatetime = result.ProcessDateTime,
                           PriceEpisodeIdentifier = result.PriceEpisodeIdentifier,
                           ApprenticeshipId =  result.ApprenticeshipId,
                           IlrActualStartDate = result.IlrStartDate,
                           IlrEffectiveFromDate = result.IlrPriceEffectiveDate,
                           IlrTotalCost = result.IlrTrainingPrice + result.IlrEndpointAssessorPrice,
                           ErrorCode = DetermineErrorCode(result.Errors),
                           Status = result.Errors.Any() ? Status.Fail : Status.Pass
                       };
        }

        private DataLockErrorCode DetermineErrorCode(Provider.Events.Api.Types.DataLockEventError[] errors)
        {
            return ListToEnumFlags<DataLockErrorCode>(
                errors.Select(m => m.ErrorCode)
                    .Select(m => m.Replace("_", ""))
                    .ToList());
        }

        public static T ListToEnumFlags<T>(List<string> enumFlagsAsList) where T : struct
        {
            if (!typeof(T).IsEnum)
                throw new NotSupportedException(typeof(T).Name + " is not an Enum");
            T flags;
            enumFlagsAsList.RemoveAll(c => !Enum.TryParse(c, true, out flags));
            var commaSeparatedFlags = string.Join(",", enumFlagsAsList);
            Enum.TryParse(commaSeparatedFlags, true, out flags);
            return flags;
        }
    }
}