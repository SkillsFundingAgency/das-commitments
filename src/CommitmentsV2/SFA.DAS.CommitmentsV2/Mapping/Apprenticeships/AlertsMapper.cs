using System.Collections.Generic;
using System.Linq;
using SFA.DAS.CommitmentsV2.Domain.Extensions;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Mapping.Apprenticeships
{
    public interface IAlertsMapper
    {
        IEnumerable<string> Map(Apprenticeship source);
    }

    public class AlertsMapper : IAlertsMapper
    {
        public IEnumerable<string> Map(Apprenticeship source)
        {
            var result = new List<string>();

            if (HasCourseDataLock(source) ||
                HasPriceDataLock(source))
            {
                result.Add("ILR data mismatch");
            }

            if (HasCourseDataLockPendingChanges(source) ||
                HasPriceDataLockPendingChanges(source))
            {
                result.Add("Changes pending");
            }

            if (HasCourseDataLockChangesRequested(source))
            {
                result.Add("Changes requested");
            }

            if (!source.PendingUpdateOriginator.HasValue) 
                return result;

            result.Add(source.PendingUpdateOriginator == Originator.Provider
                ? "Changes pending"
                : "Changes for review");

            return result;
        }

        private bool HasCourseDataLock(Apprenticeship source)
        {
            return source.DataLockStatus.Any(x => 
                x.WithCourseError() && 
                x.TriageStatus == TriageStatus.Unknown);
        }

        private bool HasPriceDataLock(Apprenticeship source)
        {
            return source.DataLockStatus.Any(x => 
                x.IsPriceOnly() && 
                x.TriageStatus == TriageStatus.Unknown);
        }

        private bool HasCourseDataLockPendingChanges(Apprenticeship source)
        {
            return source.DataLockStatus.Any(x =>
                x.WithCourseError() &&
                x.TriageStatus == TriageStatus.Change);
        }

        private bool HasPriceDataLockPendingChanges(Apprenticeship source)
        {
            return source.DataLockStatus.Any(x =>
                x.IsPriceOnly() && 
                x.TriageStatus == TriageStatus.Change);
        }

        private bool HasCourseDataLockChangesRequested(Apprenticeship source)
        {
            return source.DataLockStatus.Any(x =>
                x.WithCourseError() &&
                x.TriageStatus == TriageStatus.Restart);
        }
    }
}