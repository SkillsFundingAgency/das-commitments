using System.Collections.Generic;
using System.Linq;
using SFA.DAS.CommitmentsV2.Domain.Extensions;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;
using Apprenticeship = SFA.DAS.CommitmentsV2.Models.Apprenticeship;
using Originator = SFA.DAS.CommitmentsV2.Types.Originator;

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

            if (source.ApprenticeshipUpdate.Any(c =>
                c.Originator == (byte) Originator.Employer && c.Status == (byte) ApprenticeshipUpdateStatus.Pending))
            {
                result.Add("Changes for review");
            }
            else if (source.ApprenticeshipUpdate.Any(c =>
                c.Originator == (byte) Originator.Provider && c.Status == (byte) ApprenticeshipUpdateStatus.Pending))
            {
                result.Add("Changes pending");
            }
            
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