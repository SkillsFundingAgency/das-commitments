using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using SFA.DAS.Commitments.Api.Types.Apprenticeship;
using SFA.DAS.Commitments.Api.Types.Apprenticeship.Types;
using SFA.DAS.Commitments.Api.Types.Extensions;

namespace SFA.DAS.Commitments.Api.Orchestrators.Mappers
{
    public class ApprenticeshipFilterService
    {
        private readonly FacetMapper _facetMapper;

        public ApprenticeshipFilterService(FacetMapper facetMapper)
        {
            _facetMapper = facetMapper;
        }

        //todo: this method is too long and should be broken down!
        public virtual FilterResult Filter(IList<Apprenticeship> apprenticeships, ApprenticeshipSearchQuery apprenticeshipQuery, Originator caller)
        {
            var apps = new Apprenticeship[apprenticeships.Count];
            apprenticeships.CopyTo(apps, 0);
            IEnumerable<Apprenticeship> result = new List<Apprenticeship>(apps);

            if (apprenticeshipQuery.ApprenticeshipStatuses?.Any() ?? false)
            {    
                result = result.Where(m => apprenticeshipQuery.ApprenticeshipStatuses.Contains(_facetMapper.MapPaymentStatus(m.PaymentStatus, m.StartDate)));
            }

            if (apprenticeshipQuery.RecordStatuses?.Any() ?? false)
            {
                var records = new List<Apprenticeship>();
                if (apprenticeshipQuery.RecordStatuses.Contains(RecordStatus.NoActionNeeded))
                {
                    records.AddRange(result.Where(m => !m.DataLockPriceTriaged && !m.DataLockCourseTriaged
                        && m.PendingUpdateOriginator == null));
                }

                if (apprenticeshipQuery.RecordStatuses.Contains(RecordStatus.ChangeRequested))
                {
                    records.AddRange(result.Where(m => m.DataLockCourseTriaged));
                }

                if (apprenticeshipQuery.RecordStatuses.Contains(RecordStatus.ChangesPending))
                {
                    records.AddRange(result.Where(m => m.PendingUpdateOriginator == caller));
                    if(caller == Originator.Provider)
                        records.AddRange(result.Where(m => m.DataLockPriceTriaged));
                }

                if (apprenticeshipQuery.RecordStatuses.Contains(RecordStatus.ChangesForReview))
                {
                    records.AddRange(result.Where(m => m.PendingUpdateOriginator != null && m.PendingUpdateOriginator != caller));
                    if(caller == Originator.Employer)
                        records.AddRange(result.Where(m => m.DataLockPriceTriaged || m.DataLockCourseChangeTriaged));
                }

                if (apprenticeshipQuery.RecordStatuses.Contains(RecordStatus.IlrDataMismatch))
                {
                    records.AddRange(result.Where(m => m.DataLockPrice || m.DataLockCourse));
                }

                result = records;
            }

            if (apprenticeshipQuery.TrainingCourses?.Any() ?? false)
            {
                result = result.Where(m => apprenticeshipQuery.TrainingCourses.Contains(m.TrainingCode));
            }

            if ((apprenticeshipQuery.EmployerOrganisationIds?.Any() ?? false) && caller == Originator.Provider)
            {
                result = result.Where(m => apprenticeshipQuery.EmployerOrganisationIds.Contains(m.LegalEntityId));
            }

            if ((apprenticeshipQuery.TrainingProviderIds?.Any() ?? false) && caller == Originator.Employer)
            {
                result = result.Where(m => apprenticeshipQuery.TrainingProviderIds.Contains(m.ProviderId));
            }

            if (apprenticeshipQuery.FundingStatuses?.Any() ?? false)
            {
                // this assumes the only FundingStatus is TransferFunded (which is currently true)
                // we do this as FundingStatus.TransferFunded is a pseudo status that isn't directly stored in Apprenticeship
                result = result.Where(m => m.IsTranferFunded());
            }

            if (!string.IsNullOrWhiteSpace(apprenticeshipQuery.SearchKeyword))
            {
                var isUln = Regex.Match(apprenticeshipQuery.SearchKeyword, "^[0-9]{10}$");

                if (isUln.Success)
                {
                    result = result.Where(m => m.ULN.Equals(apprenticeshipQuery.SearchKeyword, StringComparison.InvariantCultureIgnoreCase));
                }
                else
                {
                    result = result.Where(m => m.ApprenticeshipName.ToUpper()
                        .Contains(apprenticeshipQuery.SearchKeyword.ToUpper()));
                }
            }

            var filteredResults = result.ToList();
            var totalResults = filteredResults.Count;

            var pageSize = apprenticeshipQuery.PageSize <= 0 ? 25 : apprenticeshipQuery.PageSize;
            var pageNumber = DeterminePageNumber(apprenticeshipQuery.PageNumber, pageSize, result);

            filteredResults = filteredResults
                .Skip(apprenticeshipQuery.PageSize * (pageNumber - 1))
                .Take(apprenticeshipQuery.PageSize)
                .ToList();

            return new FilterResult(totalResults, filteredResults, pageNumber, pageSize);
        }

        private static int DeterminePageNumber(int pageNumber, int pageSize, IEnumerable<Apprenticeship> approvedApprenticeships)
        {
            if (pageNumber <= 0)
                return 1;

            var totalPages = (approvedApprenticeships.Count() + pageSize - 1) / pageSize;

            if (pageNumber > totalPages)
                return totalPages;

            return pageNumber;
        }
    }

    public class FilterResult
    {
        public FilterResult(int totalResults, List<Apprenticeship> pageOfResults, int pageNumber, int pageSize)
        {
            PageOfResults = pageOfResults;
            PageNumber = pageNumber;
            PageSize = pageSize;
            TotalResults = totalResults;
        }

        public IReadOnlyCollection<Apprenticeship> PageOfResults { get; }

        public int PageNumber { get; }

        public int PageSize { get; }

        public int TotalResults { get; }
    }
}
