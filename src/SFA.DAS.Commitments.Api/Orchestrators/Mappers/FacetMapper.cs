using System;
using System.Collections.Generic;
using System.Linq;

using SFA.DAS.Commitments.Api.Types.Apprenticeship;
using SFA.DAS.Commitments.Api.Types.Apprenticeship.Types;
using SFA.DAS.Commitments.Api.Types.DataLock.Types;
using SFA.DAS.Commitments.Domain.Extensions;

namespace SFA.DAS.Commitments.Api.Orchestrators.Mappers
{
    public class FacetMapper
    {
        public Facets BuildFacetes(IList<Apprenticeship> apprenticeships, ApprenticeshipSearchQuery apprenticeshipQuery, Originator caller)
        {
            var facets = new Facets
                             {
                                 ApprenticeshipStatuses = ExtractApprenticeshipStatus(apprenticeships, apprenticeshipQuery),
                                 RecordStatuses = ExtractRecordStatus(apprenticeships, caller, apprenticeshipQuery),
                                 TrainingProviders = ExtractProviders(apprenticeships,apprenticeshipQuery),
                                 TrainingCourses = ExtractTrainingCourses(apprenticeships, apprenticeshipQuery)
                             };

            return facets;
        }

        private List<FacetItem<TrainingCourse>> ExtractTrainingCourses(IList<Apprenticeship> apprenticeships, ApprenticeshipSearchQuery apprenticeshipQuery)
        {
            var result = 
                apprenticeships
                .DistinctBy(m => m.TrainingCode)
                .ToList()
                .Select(m => new FacetItem<TrainingCourse>
                            {
                              Data  = new TrainingCourse
                                          {
                                              Id = m.TrainingCode,
                                              Name = m.TrainingName,
                                              TrainingType = m.TrainingType
                                          }
                            })
                .ToList();

            var coursIds = apprenticeshipQuery?.TrainingCourses?.Select(m => m.Id);
            result.ForEach(m => m.Selected =  coursIds?.Contains(m.Data.Id) ?? false);

            return result;
        }

        private List<FacetItem<string>> ExtractProviders(IList<Apprenticeship> apprenticeships, ApprenticeshipSearchQuery apprenticeshipQuery)
        {
            var providers = 
                apprenticeships
                .Select(m => m.ProviderName)
                .Distinct()
                .Select(m => new FacetItem<string>()
                            {
                                Data = m,
                                Selected = false
                            })
                .ToList();

            providers.ForEach(m => m.Selected = apprenticeshipQuery.TrainingProviders?.Contains(m.Data) ?? false);

            return providers;

        }

        private List<FacetItem<RecordStatus>> ExtractRecordStatus(IList<Apprenticeship> apprenticeships, Originator caller, ApprenticeshipSearchQuery apprenticeshipQuery)
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

        private List<FacetItem<ApprenticeshipStatus>> ExtractApprenticeshipStatus(IList<Apprenticeship> apprenticeships, ApprenticeshipSearchQuery apprenticeshipQuery)
        {
            var er = apprenticeships.Select(m =>
                new FacetItem<ApprenticeshipStatus>
                {
                    Data = MapPaymentStatus(m.PaymentStatus, m.StartDate),
                    Selected = false
                }
            )
            //.Where(m => m.Data != ApprenticeshipStatus.None)
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
                    return ApprenticeshipStatus.WaitingToStart;
            }
        }
    }
}