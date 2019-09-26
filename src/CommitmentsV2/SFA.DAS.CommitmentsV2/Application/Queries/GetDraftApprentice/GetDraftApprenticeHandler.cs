using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Data.QueryExtensions;
using SFA.DAS.CommitmentsV2.Authentication;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetDraftApprentice
{
    public class GetDraftApprenticeHandler : IRequestHandler<GetDraftApprenticeRequest, GetDraftApprenticeResponse>
    {
        private readonly Lazy<CommitmentsDbContext> _dbContext;
        private readonly IAuthenticationService _authenticationService;

        public GetDraftApprenticeHandler(Lazy<CommitmentsDbContext> dbContext, IAuthenticationService authenticationService)
        {
            _dbContext = dbContext;
            _authenticationService = authenticationService;
        }

        public async Task<GetDraftApprenticeResponse> Handle(GetDraftApprenticeRequest request, CancellationToken cancellationToken)
        {
            var requestingParty = _authenticationService.GetUserParty();

            var x = await _dbContext.Value
                .DraftApprenticeships.GetById(
                    request.CohortId,
                    request.DraftApprenticeshipId,
                    draft => new GetDraftApprenticeResponse
                    {
                        CourseCode = draft.CourseCode,
                        StartDate = draft.StartDate,
                        Id = draft.Id,
                        Cost = (int?)draft.Cost,
                        DateOfBirth = draft.DateOfBirth,
                        EndDate = draft.EndDate,
                        FirstName = draft.FirstName,
                        LastName = draft.LastName,
                        Reference = requestingParty == Party.Provider ? draft.ProviderRef : draft.EmployerRef,
                        ReservationId = draft.ReservationId,
                        Uln = draft.Uln
                    },
                    cancellationToken);

            return x;
        }
    }
}
