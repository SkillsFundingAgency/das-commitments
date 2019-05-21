using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Data.QueryExtensions;
using SFA.DAS.CommitmentsV2.Domain.Entities;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetDraftApprentice
{
    public class GetDraftApprenticeHandler : IRequestHandler<GetDraftApprenticeRequest, GetDraftApprenticeResponse>
    {
        private readonly Lazy<ProviderCommitmentsDbContext> _dbContext;

        public GetDraftApprenticeHandler(Lazy<ProviderCommitmentsDbContext> dbContext)
        {
            _dbContext = dbContext;
        }

        public Task<GetDraftApprenticeResponse> Handle(GetDraftApprenticeRequest request, CancellationToken cancellationToken)
        {

            return _dbContext.Value
                .DraftApprenticeships.GetById(
                    request.CohortId,
                    request.DraftApprenticeshipId,
                    draft => new GetDraftApprenticeResponse
                    {
                        CourseCode = draft.CourseCode,
                        StartDate = draft.StartDate,
                        Id = draft.Id,
                        Cost = (int)draft.Cost,
                        DateOfBirth = draft.DateOfBirth,
                        EndDate = draft.EndDate,
                        FirstName = draft.FirstName,
                        LastName = draft.LastName,
                        Reference = draft.ProviderRef,
                        ReservationId = draft.ReservationId,
                        Uln = draft.Uln
                    },
                    cancellationToken);

        }
    }
}
