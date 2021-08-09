using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Data.QueryExtensions;
using SFA.DAS.CommitmentsV2.Authentication;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetDraftApprenticeship
{
    public class GetDraftApprenticeshipQueryHandler : IRequestHandler<GetDraftApprenticeshipQuery, GetDraftApprenticeshipQueryResult>
    {
        private readonly Lazy<ProviderCommitmentsDbContext> _dbContext;
        private readonly IAuthenticationService _authenticationService;

        public GetDraftApprenticeshipQueryHandler(Lazy<ProviderCommitmentsDbContext> dbContext, IAuthenticationService authenticationService)
        {
            _dbContext = dbContext;
            _authenticationService = authenticationService;
        }

        public async Task<GetDraftApprenticeshipQueryResult> Handle(GetDraftApprenticeshipQuery request, CancellationToken cancellationToken)
        {
            var requestingParty = _authenticationService.GetUserParty();

            var x = await _dbContext.Value
                .DraftApprenticeships.GetById(
                    request.CohortId,
                    request.DraftApprenticeshipId,
                    draft => new GetDraftApprenticeshipQueryResult
                    {
                        CourseCode = draft.CourseCode,
                        TrainingCourseVersion = draft.TrainingCourseVersion,
                        TrainingCourseName = draft.CourseName,
                        TrainingCourseOption = draft.TrainingCourseOption,
                        StandardUId = draft.StandardUId,
                        StartDate = draft.StartDate,
                        Id = draft.Id,
                        Cost = (int?)draft.Cost,
                        DateOfBirth = draft.DateOfBirth,
                        EndDate = draft.EndDate,
                        FirstName = draft.FirstName,
                        LastName = draft.LastName,
                        Email = draft.Email,
                        Reference = requestingParty == Party.Provider ? draft.ProviderRef : draft.EmployerRef,
                        ReservationId = draft.ReservationId,
                        Uln = draft.Uln,
                        IsContinuation = draft.ContinuationOfId.HasValue,
                        OriginalStartDate = draft.OriginalStartDate
                    },
                    cancellationToken);

            return x;
        }
    }
}
