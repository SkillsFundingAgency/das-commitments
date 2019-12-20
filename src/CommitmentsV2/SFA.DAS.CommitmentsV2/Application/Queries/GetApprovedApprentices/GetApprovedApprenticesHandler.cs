using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Mapping;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetApprovedApprentices
{
    public class GetApprovedApprenticesHandler : IRequestHandler<GetApprovedApprenticesRequest, GetApprovedApprenticesResponse>
    {
        private readonly IProviderCommitmentsDbContext _dbContext;
        private readonly IMapper<ApprovedApprenticeship, ApprenticeshipDetails> _mapper;

        public GetApprovedApprenticesHandler(
            IProviderCommitmentsDbContext dbContext,
            IMapper<ApprovedApprenticeship, ApprenticeshipDetails> mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public async Task<GetApprovedApprenticesResponse> Handle(GetApprovedApprenticesRequest request, CancellationToken cancellationToken)
        {
            var mapped = new List<ApprenticeshipDetails>();

            var matched = await _dbContext.ApprovedApprenticeships
                .Include(apprenticeship => apprenticeship.Cohort)
                .Include(apprenticeship => apprenticeship.DataLockStatus)
                .Where(apprenticeship => apprenticeship.Cohort.ProviderId == request.ProviderId)
                .ToListAsync(cancellationToken);

            foreach (var apprenticeship in matched)
            {
                var details = await _mapper.Map(apprenticeship);
                mapped.Add(details);
            }

            return new GetApprovedApprenticesResponse
            {
                Apprenticeships = SortApprenticeships(mapped)
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
                new List<ApprenticeshipDetails>(apprenticeshipsWithAlerts.OrderBy(x => x.ApprenticeFirstName)
                    .ThenBy(x => x.Uln)
                    .ThenBy(x => x.EmployerName)
                    .ThenBy(x => x.CourseName)
                    .ThenBy(x => x.PlannedStartDate));

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
                new List<ApprenticeshipDetails>(apprenticeshipsWithoutAlerts.OrderBy(x => x.ApprenticeFirstName)
                    .ThenBy(x => x.Uln)
                    .ThenBy(x => x.EmployerName)
                    .ThenBy(x => x.CourseName)
                    .ThenBy(x => x.PlannedStartDate));
           
            return apprenticeshipsWithoutAlertsSortedByName;
        }
    }
}
