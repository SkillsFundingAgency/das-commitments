using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetApprovedApprentices
{
    public class GetApprovedApprenticesHandler : IRequestHandler<GetApprovedApprenticesRequest, GetApprovedApprenticesResponse>
    {
        private readonly IProviderCommitmentsDbContext _dbContext;

        public GetApprovedApprenticesHandler(IProviderCommitmentsDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<GetApprovedApprenticesResponse> Handle(GetApprovedApprenticesRequest request, CancellationToken cancellationToken)
        {
            var matched = await _dbContext.ApprovedApprenticeships
                .Include(apprenticeship => apprenticeship.Cohort)
                .Include(apprenticeship => apprenticeship.DataLockStatus)
                .Where(apprenticeship => apprenticeship.ProviderRef == request.ProviderId.ToString())
                .Select(apprenticeship => (ApprenticeshipDetails)apprenticeship)
                .ToListAsync(cancellationToken);

            return new GetApprovedApprenticesResponse
            {
                Apprenticeships = SortApprenticeships(matched)
            };
        }

        private List<ApprenticeshipDetails> SortApprenticeships(List<ApprenticeshipDetails> apprenticeships)
        {
            var sortedApprenticeships = new List<ApprenticeshipDetails>();

            var apprenticeshipsWithAlerts = SortApprenticeshipsWithAlerts(apprenticeships);

            foreach (var apprenticeship in apprenticeshipsWithAlerts)
            {
                sortedApprenticeships.Add(apprenticeship);
            }

            var apprenticeshipsWithoutAlerts = SortApprenticeshipsWithoutAlerts(apprenticeships);

            foreach (var apprenticeship in apprenticeshipsWithoutAlerts)
            {
                sortedApprenticeships.Add(apprenticeship);
            }

            return sortedApprenticeships;
        }

        private List<ApprenticeshipDetails> SortApprenticeshipsWithAlerts(List<ApprenticeshipDetails> apprenticeships)
        {
            var apprenticeshipsWithAlerts = new List<ApprenticeshipDetails>();
            foreach (var apprenticeship in apprenticeships)
            {
                if (apprenticeship.Alerts != null)
                { apprenticeshipsWithAlerts.Add(apprenticeship);}
            }

            var apprenticeshipsWithAlertsSortedByName = 
                new List<ApprenticeshipDetails>(apprenticeshipsWithAlerts.OrderBy(x => x.ApprenticeFirstName));

            apprenticeshipsWithAlertsSortedByName = CheckForUniqueness(apprenticeshipsWithAlertsSortedByName, true);

            return apprenticeshipsWithAlertsSortedByName;
        }

        private List<ApprenticeshipDetails> SortApprenticeshipsWithoutAlerts(List<ApprenticeshipDetails> apprenticeships)
        {
            var apprenticeshipsWithoutAlerts = new List<ApprenticeshipDetails>();
            foreach (var apprenticeship in apprenticeships)
            {
                if (apprenticeship.Alerts == null)
                { apprenticeshipsWithoutAlerts.Add(apprenticeship); }
            }

            var apprenticeshipsWithoutAlertsSortedByName =
                new List<ApprenticeshipDetails>(apprenticeshipsWithoutAlerts.OrderBy(x => x.ApprenticeFirstName));

            apprenticeshipsWithoutAlertsSortedByName = CheckForUniqueness(apprenticeshipsWithoutAlertsSortedByName, false);

            return apprenticeshipsWithoutAlertsSortedByName;
        }

        private List<ApprenticeshipDetails> CheckForUniqueness(List<ApprenticeshipDetails> apprenticeships, bool hasAlerts)
        {
            for (int i = 0; i < apprenticeships.Count -1; i++)
            {
                if (hasAlerts && string.Equals(apprenticeships[i].ApprenticeFirstName, apprenticeships[i + 1].ApprenticeFirstName, StringComparison.CurrentCultureIgnoreCase))
                {
                    if (apprenticeships[i].Alerts.FirstOrDefault() != apprenticeships[1 + 1].Alerts.FirstOrDefault())
                    {
                        return apprenticeships;
                    }
                }
                if (apprenticeships[i].ApprenticeFirstName.ToUpper() == apprenticeships[i + 1].ApprenticeFirstName.ToUpper())
                {
                    var comparedApprenticeships = CompareByUln(apprenticeships[i], apprenticeships[i + 1], hasAlerts);
                    apprenticeships[i] = comparedApprenticeships[0];
                    apprenticeships[i + 1] = comparedApprenticeships[1];
                }
            }

            return apprenticeships;
        }

        private List<ApprenticeshipDetails> CompareByUln(ApprenticeshipDetails apprenticeship1,
            ApprenticeshipDetails apprenticeship2, bool hasAlerts)
        {
            if (hasAlerts && apprenticeship1.Uln == apprenticeship2.Uln)
            {
                if (apprenticeship1.Alerts.FirstOrDefault() != apprenticeship2.Alerts.FirstOrDefault())
                {
                    return new List<ApprenticeshipDetails>{apprenticeship1,apprenticeship2};
                }
            }

            var sortedApprenticeships = new List<ApprenticeshipDetails>();

            if (apprenticeship1.Uln == apprenticeship2.Uln)
            {
                sortedApprenticeships = CompareByEmployerName(apprenticeship1, apprenticeship2, hasAlerts);
            }
            else
            {
                var unsortedApprenticeships = new List<ApprenticeshipDetails> { apprenticeship1, apprenticeship2 };
                sortedApprenticeships = new List<ApprenticeshipDetails>(unsortedApprenticeships.OrderBy(x => x.Uln));
            }

            return sortedApprenticeships;
        }

        private List<ApprenticeshipDetails> CompareByEmployerName(ApprenticeshipDetails apprenticeship1,
            ApprenticeshipDetails apprenticeship2, bool hasAlerts)
        {
            if (hasAlerts && string.Equals(apprenticeship1.EmployerName, apprenticeship2.EmployerName, StringComparison.CurrentCultureIgnoreCase))
            {
                if (apprenticeship1.Alerts.FirstOrDefault() != apprenticeship2.Alerts.FirstOrDefault())
                {
                    return new List<ApprenticeshipDetails> { apprenticeship1, apprenticeship2 };
                }
            }

            var sortedApprenticeships = new List<ApprenticeshipDetails>();

            if (string.Equals(apprenticeship1.EmployerName, apprenticeship2.EmployerName, StringComparison.CurrentCultureIgnoreCase))
            {
                sortedApprenticeships = CompareByApprenticeshipCourseName(apprenticeship1, apprenticeship2, hasAlerts);
            }
            else
            {
                var unsortedApprenticeships = new List<ApprenticeshipDetails> { apprenticeship1, apprenticeship2 };
                sortedApprenticeships = new List<ApprenticeshipDetails>(unsortedApprenticeships.OrderBy(x => x.EmployerName));
            }

            return sortedApprenticeships;
        }

        private List<ApprenticeshipDetails> CompareByApprenticeshipCourseName(ApprenticeshipDetails apprenticeship1,
            ApprenticeshipDetails apprenticeship2, bool hasAlerts)
        {
            if (hasAlerts && string.Equals(apprenticeship1.CourseName, apprenticeship2.CourseName, StringComparison.CurrentCultureIgnoreCase))
            {
                if (apprenticeship1.Alerts.FirstOrDefault() != apprenticeship2.Alerts.FirstOrDefault())
                {
                    return new List<ApprenticeshipDetails> { apprenticeship1, apprenticeship2 };
                }
            }

            var sortedApprenticeships = new List<ApprenticeshipDetails>();

            if (string.Equals(apprenticeship1.CourseName, apprenticeship2.CourseName, StringComparison.CurrentCultureIgnoreCase))
            {
                sortedApprenticeships = CompareByPlannedStartDate(apprenticeship1, apprenticeship2, hasAlerts);
            }
            else
            {
                var unsortedApprenticeships = new List<ApprenticeshipDetails> { apprenticeship1, apprenticeship2 };
                sortedApprenticeships = new List<ApprenticeshipDetails>(unsortedApprenticeships.OrderBy(x => x.CourseName));
            }

            return sortedApprenticeships;
        }

        private List<ApprenticeshipDetails> CompareByPlannedStartDate(ApprenticeshipDetails apprenticeship1,
            ApprenticeshipDetails apprenticeship2, bool hasAlerts)
        {
            if (hasAlerts && apprenticeship1.PlannedStartDate == apprenticeship2.PlannedStartDate)
            {
                if (apprenticeship1.Alerts.FirstOrDefault() != apprenticeship2.Alerts.FirstOrDefault())
                {
                    return new List<ApprenticeshipDetails> { apprenticeship1, apprenticeship2 };
                }
            }
            var unsortedApprenticeships = new List<ApprenticeshipDetails> {apprenticeship1, apprenticeship2};
            var sortedApprenticeships = new List<ApprenticeshipDetails>(unsortedApprenticeships.OrderBy(x => x.Uln));
            
            return sortedApprenticeships;
        }
    }
}
