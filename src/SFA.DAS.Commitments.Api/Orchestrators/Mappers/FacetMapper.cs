using System;
using System.Collections.Generic;
using System.Linq;

using SFA.DAS.Commitments.Api.Models;
using SFA.DAS.Commitments.Api.Types.Apprenticeship;
using SFA.DAS.Commitments.Api.Types.Apprenticeship.Types;
using SFA.DAS.Commitments.Api.Types.DataLock.Types;
using SFA.DAS.Commitments.Domain.Extensions;

namespace SFA.DAS.Commitments.Api.Orchestrators.Mappers
{
    public class FacetMapper
    {
        public Facets BuildFacetes(IList<Apprenticeship> apprenticeships, ApprenticeshipQuery apprenticeshipQuery, Originator caller)
        {
            var facets = new Facets
                             {
                                 ApprenticeshipStatuses = ExtractApprenticeshipStatus(apprenticeships, apprenticeshipQuery),
                                 RecordStatuses = ExtractRecordStatus(apprenticeships, caller, apprenticeshipQuery)
                             };

            return facets;
        }

        private List<FacetItem<RecordStatus>> ExtractRecordStatus(IList<Apprenticeship> apprenticeships, Originator caller, ApprenticeshipQuery apprenticeshipQuery)
        {
            var result = apprenticeships
                .Where(m => m.PendingUpdateOriginator != null)
                .Select(m => new FacetItem<RecordStatus>
                        {
                            Data =
                                m.PendingUpdateOriginator == caller
                                    ? RecordStatus.ChangesPending
                                    : RecordStatus.ChangesForReview
                        });

            var resultDataLock = apprenticeships
                .Where(m => m.DataLockTriageStatus == TriageStatus.Restart)
                .Select(m => new FacetItem<RecordStatus>
                                {
                                    Data = RecordStatus.ChangeRequested
                                     
                                });

            var concatResult = result.Concat(resultDataLock).DistinctBy(m => m.Data).ToList();

            concatResult.ForEach(m => m.Selected = apprenticeshipQuery.RecordStatuses?.Contains(m.Data) ?? false);

            return concatResult;
        }

        private List<FacetItem<ApprenticeshipStatus>> ExtractApprenticeshipStatus(IList<Apprenticeship> apprenticeships, ApprenticeshipQuery apprenticeshipQuery)
        {
            var er = apprenticeships.Select(m =>
                new FacetItem<ApprenticeshipStatus>
                {
                    Data = MapPaymentStatus(m.PaymentStatus, m.StartDate),
                    Selected = false
                }
            ).Where(m => m.Data != ApprenticeshipStatus.None)
            .DistinctBy(m => m.Data)
            .ToList();

            er.ForEach(m => m.Selected = apprenticeshipQuery.ApprenticeshipStatuses?.Contains(m.Data) ?? false);

            return er;
        }

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
                    return ApprenticeshipStatus.Live;
                default:
                    return ApprenticeshipStatus.None;
            }
        }
    }
}