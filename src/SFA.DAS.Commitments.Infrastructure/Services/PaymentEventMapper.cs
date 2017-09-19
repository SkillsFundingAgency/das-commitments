﻿using System;
using System.Collections.Generic;
using System.Linq;

using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Entities.DataLock;
using SFA.DAS.Provider.Events.Api.Types;

using EventStatus = SFA.DAS.Commitments.Domain.Entities.EventStatus;

namespace SFA.DAS.Commitments.Infrastructure.Services
{
    public class PaymentEventMapper : IPaymentEventMapper
    {
        public DataLockStatus Map(DataLockEvent dataLockEvent)
        {
            return new DataLockStatus
                       {
                           DataLockEventId = dataLockEvent.Id,
                           DataLockEventDatetime = dataLockEvent.ProcessDateTime,
                           PriceEpisodeIdentifier = dataLockEvent.PriceEpisodeIdentifier,
                           ApprenticeshipId =  dataLockEvent.ApprenticeshipId,
                           IlrTrainingCourseCode = DeriveTrainingCourseCode(dataLockEvent),
                           IlrTrainingType = DeriveTrainingType(dataLockEvent),
                           IlrActualStartDate = dataLockEvent.IlrStartDate,
                           IlrEffectiveFromDate = dataLockEvent.IlrPriceEffectiveFromDate,
                           IlrPriceEffectiveToDate = dataLockEvent.IlrPriceEffectiveToDate,
                           IlrTotalCost = dataLockEvent.IlrTrainingPrice + dataLockEvent.IlrEndpointAssessorPrice,
                           ErrorCode = DetermineErrorCode(dataLockEvent.Errors),
                           Status = dataLockEvent.Errors?.Any() ?? false ? Status.Fail : Status.Pass,
                           EventStatus = (EventStatus)dataLockEvent.Status
                       };
        }

        private TrainingType DeriveTrainingType(DataLockEvent dataLockEvent)
        {
            return dataLockEvent.IlrProgrammeType == 25 
                ? TrainingType.Standard 
                : TrainingType.Framework;
        }

        private string DeriveTrainingCourseCode(DataLockEvent dataLockEvent)
        {
            return dataLockEvent.IlrProgrammeType == 25 
                ? $"{dataLockEvent.IlrStandardCode}" : 
                $"{dataLockEvent.IlrFrameworkCode}-{dataLockEvent.IlrProgrammeType}-{dataLockEvent.IlrPathwayCode}";
        }

        private DataLockErrorCode DetermineErrorCode(Provider.Events.Api.Types.DataLockEventError[] errors)
        {
            return ListToEnumFlags<DataLockErrorCode>(
                errors?.Select(m => m.ErrorCode)
                    .Select(m => m.Replace("_", ""))
                    .ToList());
        }

        public static T ListToEnumFlags<T>(List<string> enumFlagsAsList) where T : struct
        {
            if (!typeof(T).IsEnum)
                throw new NotSupportedException(typeof(T).Name + " is not an Enum");
            T flags;
            if(enumFlagsAsList == null)
                return default(T);

            enumFlagsAsList.RemoveAll(c => !Enum.TryParse(c, true, out flags));
            var commaSeparatedFlags = string.Join(",", enumFlagsAsList);
            Enum.TryParse(commaSeparatedFlags, true, out flags);
            return flags;
        }
    }
}