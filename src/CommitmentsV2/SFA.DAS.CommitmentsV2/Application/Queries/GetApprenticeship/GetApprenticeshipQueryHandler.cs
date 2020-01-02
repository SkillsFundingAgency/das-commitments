using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SFA.DAS.CommitmentsV2.Authentication;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Data.QueryExtensions;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeship
{
    public class GetApprenticeshipQueryHandler: IRequestHandler<GetApprenticeshipQuery, GetApprenticeshipQueryResult>
    {
        private readonly Lazy<ProviderCommitmentsDbContext> _dbContext;
        private readonly IAuthenticationService _authenticationService;

        public GetApprenticeshipQueryHandler(Lazy<ProviderCommitmentsDbContext> dbContext, IAuthenticationService authenticationService)
        {
            _dbContext = dbContext;
            _authenticationService = authenticationService;
        }


        public async Task<GetApprenticeshipQueryResult> Handle(GetApprenticeshipQuery request, CancellationToken cancellationToken)
        {
            var requestingParty = _authenticationService.GetUserParty();

            var x = await _dbContext.Value
                .ApprovedApprenticeships.GetById(request.ApprenticeshipId, apprenticeship => 
                    new GetApprenticeshipQueryResult
                    {
                        Id = apprenticeship.Id,
                        CourseCode = apprenticeship.CourseCode
                        //StartDate = apprenticeship.StartDate.Value,
                        //DateOfBirth = apprenticeship.DateOfBirth.Value,
                        //EndDate = apprenticeship.EndDate,
                        //FirstName = apprenticeship.FirstName,
                        //LastName = apprenticeship.LastName,
                        //Reference = requestingParty == Party.Provider ? apprenticeship.ProviderRef : apprenticeship.EmployerRef,
                        //Uln = apprenticeship.Uln
                    },
                    cancellationToken);

            return x;
        }
    }
}
