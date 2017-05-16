using System;
using System.Collections.Generic;
using System.Linq;

using SFA.DAS.Commitments.Api.Types.Apprenticeship;
using SFA.DAS.Commitments.Api.Types.Apprenticeship.Types;
using SFA.DAS.Commitments.Api.Types.DataLock.Types;

namespace SFA.DAS.Commitments.Application.Services
{
    public class ApprenticeshipFilterService
    {
        public IEnumerable<Apprenticeship> Filter(IList<Apprenticeship> apprenticeships, ApprenticeshipSearchQuery apprenticeshipQuery, Originator caller)
        {
            var apps = new Apprenticeship[apprenticeships.Count];
            apprenticeships.CopyTo(apps, 0);
            IEnumerable<Apprenticeship> result = new List<Apprenticeship>(apps);

            if (apprenticeshipQuery.ApprenticeshipStatuses?.Any() ?? false)
            {    
                result = result.Where(m => apprenticeshipQuery.ApprenticeshipStatuses.Contains(MapPaymentStatus(m.PaymentStatus, m.StartDate)));
            }

            if (apprenticeshipQuery.RecordStatuses?.Any() ?? false)
            {
                if (apprenticeshipQuery.RecordStatuses.Contains(RecordStatus.ChangeRequested))
                {
                    result = result.Where(m => m.DataLockTriageStatus == TriageStatus.Restart);
                }

                if (apprenticeshipQuery.RecordStatuses.Contains(RecordStatus.ChangesPending))
                {
                    result = result.Where(m => m.PendingUpdateOriginator == caller);
                }

                if (apprenticeshipQuery.RecordStatuses.Contains(RecordStatus.ChangesForReview))
                {
                    result = result.Where(m => m.PendingUpdateOriginator != null && m.PendingUpdateOriginator != caller);
                }
            }

            if (apprenticeshipQuery.TrainingCourses?.Any() ?? false)
            {
                result = result.Where(m => apprenticeshipQuery.TrainingCourses.Contains(m.TrainingCode));
            }

            if ((apprenticeshipQuery.EmployerOrganisationIds?.Any() ?? false) && caller == Originator.Provider)
            {
                result = result.Where(m => apprenticeshipQuery.EmployerOrganisationIds.Contains(m.EmployerAccountId));
            }

            if ((apprenticeshipQuery.TrainingProviderIds?.Any() ?? false) && caller == Originator.Employer)
            {
                result = result.Where(m => apprenticeshipQuery.TrainingProviderIds.Contains(m.ProviderId));
            }

            return result;
        }

        // ToDo: Move to common mapper
        private ApprenticeshipStatus MapPaymentStatus(PaymentStatus paymentStatus, DateTime? apprenticeshipStartDate)
        {
            var now = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var waitingToStart = apprenticeshipStartDate.HasValue && apprenticeshipStartDate.Value > now;

            switch (paymentStatus)
            {
                case PaymentStatus.Active:
                    return waitingToStart ? ApprenticeshipStatus.WaitingToStart : ApprenticeshipStatus.Live;
                case PaymentStatus.Paused:
                    return ApprenticeshipStatus.Paused;
                case PaymentStatus.Withdrawn:
                    return ApprenticeshipStatus.Stopped;
                case PaymentStatus.Completed:
                    return ApprenticeshipStatus.Finished;
                case PaymentStatus.Deleted:
                    return ApprenticeshipStatus.WaitingToStart;
                default:
                    return ApprenticeshipStatus.WaitingToStart;
            }
        }
    }
}
