using System;
using System.Collections.Generic;
using System.Linq;

using SFA.DAS.Commitments.Api.Types.Apprenticeship;
using SFA.DAS.Commitments.Api.Types.Apprenticeship.Types;
using SFA.DAS.Commitments.Api.Types.Extensions;
using SFA.DAS.Commitments.Domain.Extensions;
using SFA.DAS.Commitments.Domain.Interfaces;

namespace SFA.DAS.Commitments.Api.Orchestrators.Mappers
{
    public class FacetMapper
    {
        private readonly ICurrentDateTime _currentDateTime;

        public FacetMapper(ICurrentDateTime currentDateTime)
        {
            _currentDateTime = currentDateTime;
        }

        public virtual Facets BuildFacets(IList<Apprenticeship> apprenticeships, ApprenticeshipSearchQuery apprenticeshipQuery, Originator caller)
        {
            var facets = new Facets
                             {
                                 ApprenticeshipStatuses = ExtractApprenticeshipStatus(apprenticeships, apprenticeshipQuery),
                                 RecordStatuses = ExtractRecordStatus(apprenticeships, caller, apprenticeshipQuery),
                                 TrainingProviders = ExtractProviders(apprenticeships, apprenticeshipQuery),
                                 EmployerOrganisations = ExtractEmployers(apprenticeships, apprenticeshipQuery),
                                 TrainingCourses = ExtractTrainingCourses(apprenticeships, apprenticeshipQuery),
                                 FundingStatuses = ExtractFundingStatuses(apprenticeships, apprenticeshipQuery)
                             };

            return facets;
        }

        public ApprenticeshipStatus MapPaymentStatus(PaymentStatus paymentStatus, DateTime? apprenticeshipStartDate)
        {
            var now = new DateTime(_currentDateTime.Now.Year, _currentDateTime.Now.Month, 1);
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

        private List<FacetItem<EmployerOrganisation>> ExtractEmployers(IList<Apprenticeship> apprenticeships, ApprenticeshipSearchQuery apprenticeshipQuery)
        {
            var employers =
                apprenticeships
                .DistinctBy(m => m.LegalEntityId)
                .Select(m => new FacetItem<EmployerOrganisation>()
                {
                    Data = new EmployerOrganisation()
                    {
                        Id = m.LegalEntityId,
                        Name = m.LegalEntityName
                    },
                    Selected = false
                })
                .ToList();

            employers.ForEach(m => m.Selected = apprenticeshipQuery?.EmployerOrganisationIds?.Contains(m.Data.Id) ?? false);

            return employers;
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
            
            result.ForEach(m => m.Selected = apprenticeshipQuery?.TrainingCourses?.Contains(m.Data.Id) ?? false);

            return result;
        }

        private List<FacetItem<TrainingProvider>> ExtractProviders(IList<Apprenticeship> apprenticeships, ApprenticeshipSearchQuery apprenticeshipQuery)
        {
            var providers = 
                apprenticeships
                .DistinctBy(m => m.ProviderId)
                .Select(m => new FacetItem<TrainingProvider>()
                            {
                                Data = new TrainingProvider
                                           {
                                               Id = m.ProviderId,
                                               Name = m.ProviderName
                                            },
                                Selected = false
                            })
                .ToList();

            providers.ForEach(m => m.Selected = apprenticeshipQuery?.TrainingProviderIds?.Contains(m.Data.Id) ?? false);

            return providers;

        }

        private List<FacetItem<RecordStatus>> ExtractRecordStatus(IList<Apprenticeship> apprenticeships, Originator caller, ApprenticeshipSearchQuery apprenticeshipQuery)
        {
            Action<List<FacetItem<RecordStatus>>, RecordStatus> addUnique = (x, y) =>
            {
                x.Add(new FacetItem<RecordStatus> { Data = y });
            };

            var result = apprenticeships
                .Where(m => m.PendingUpdateOriginator != null)
                .Select(m => new FacetItem<RecordStatus>
                        {
                            Data =
                                m.PendingUpdateOriginator == caller
                                    ? RecordStatus.ChangesPending
                                    : RecordStatus.ChangesForReview
                        });

            var statusFacets = new List<FacetItem<RecordStatus>>();
            if (apprenticeships.Any(x => (x.DataLockPrice || x.DataLockCourse)))
            {
                addUnique(statusFacets,
                    caller == Originator.Provider ? RecordStatus.IlrDataMismatch : RecordStatus.NoActionNeeded);
            }

            if (apprenticeships.Any(x => x.DataLockPriceTriaged || x.DataLockCourseChangeTriaged))
            {
                addUnique(statusFacets,
                    caller == Originator.Provider ? RecordStatus.ChangesPending : RecordStatus.ChangesForReview);
            }

            if (apprenticeships.Any(x => x.DataLockCourseTriaged))
            {
                addUnique(statusFacets, RecordStatus.ChangeRequested);
            }


            List<FacetItem<RecordStatus>> concatResult = result.Concat(statusFacets).DistinctBy(m => m.Data).ToList();

            concatResult.ForEach(m => m.Selected = apprenticeshipQuery.RecordStatuses?.Contains(m.Data) ?? false);

            return concatResult;
        }

        private List<FacetItem<ApprenticeshipStatus>> ExtractApprenticeshipStatus(IList<Apprenticeship> apprenticeships, ApprenticeshipSearchQuery apprenticeshipQuery)
        {
            var er = apprenticeships.Select(m =>
                new FacetItem<ApprenticeshipStatus>
                {
                    Data = MapPaymentStatus(m.PaymentStatus, m.StartDate)
                }
            )
            .DistinctBy(m => m.Data)
            .ToList();

            er.ForEach(m => m.Selected = apprenticeshipQuery?.ApprenticeshipStatuses?.Contains(m.Data) ?? false);

            return er;
        }

        private List<FacetItem<FundingStatus>> ExtractFundingStatuses(IEnumerable<Apprenticeship> apprenticeships, ApprenticeshipSearchQuery apprenticeshipQuery)
        {
            var facets = new List<FacetItem<FundingStatus>>();

            if (apprenticeships.Any(a => a.IsTranferFunded()))
            {
                facets.Add(new FacetItem<FundingStatus>
                {
                    Data = FundingStatus.TransferFunded,
                    //Selected = apprenticeshipQuery?.FundingStatuses.Contains(FundingStatus.TransferFunded) ?? false

                    // this assumes the only FundingStatus is TransferFunded (which is currently true)
                    Selected = apprenticeshipQuery?.FundingStatuses?.Any() ?? false
                });
            }

            return facets;
        }
    }
}
