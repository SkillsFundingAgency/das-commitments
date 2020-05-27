using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SFA.DAS.CommitmentsV2.Authentication;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Data.QueryExtensions;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.Encoding;

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
                .Apprenticeships
                .GetById(request.ApprenticeshipId, apprenticeship => 
                    new GetApprenticeshipQueryResult
                    {
                        Id = apprenticeship.Id,
                        CohortId = apprenticeship.CommitmentId,
                        CourseCode = apprenticeship.CourseCode,
                        CourseName = apprenticeship.CourseName,
                        EmployerAccountId = apprenticeship.Cohort.EmployerAccountId,
                        AccountLegalEntityId = apprenticeship.Cohort.AccountLegalEntityId,
                        EmployerName = apprenticeship.Cohort.AccountLegalEntity.Name,
                        ProviderId = apprenticeship.Cohort.ProviderId,
                        ProviderName = apprenticeship.Cohort.Provider.Name,
                        DateOfBirth = apprenticeship.DateOfBirth.Value,
                        FirstName = apprenticeship.FirstName,
                        LastName = apprenticeship.LastName,
                        Uln = apprenticeship.Uln,
                        StartDate = apprenticeship.StartDate.Value,
                        EndDate = apprenticeship.EndDate.Value,
                        EndpointAssessorName = apprenticeship.EpaOrg.Name,
                        Reference = requestingParty == Party.Provider ? apprenticeship.ProviderRef : apprenticeship.EmployerRef,
                        Status = apprenticeship.GetApprenticeshipStatus(null),
                        StopDate = apprenticeship.StopDate,
                        PauseDate = apprenticeship.PauseDate,
                        HasHadDataLockSuccess = apprenticeship.HasHadDataLockSuccess,
                        CompletionDate = apprenticeship.CompletionDate,
                        ContinuationOfId = apprenticeship.ContinuationOfId,
                        OriginalStartDate = apprenticeship.OriginalStartDate
                    },
                    cancellationToken);

            return x;
        }
    }
}


