using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Messages.Events;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetDraftApprenticeshipCreatedEventsForCohort
{
    public class GetDraftApprenticeshipCreatedEventsForCohortQueryHandler : IRequestHandler<GetDraftApprenticeshipCreatedEventsForCohortQuery, GetDraftApprenticeshipCreatedEventsForCohortQueryResult>
    {
        private readonly Lazy<ProviderCommitmentsDbContext> _db;

        public GetDraftApprenticeshipCreatedEventsForCohortQueryHandler(Lazy<ProviderCommitmentsDbContext> db)
        {
            _db = db;
        }

        public async Task<GetDraftApprenticeshipCreatedEventsForCohortQueryResult> Handle(GetDraftApprenticeshipCreatedEventsForCohortQuery command, CancellationToken cancellationToken)
        {
            var cohort = await _db.Value.Cohorts.Include(c => c.Apprenticeships)
                .SingleAsync(x => x.Id == command.CohortId, cancellationToken).ConfigureAwait(false);

            if (cohort.ProviderId != command.ProviderId)
            {
                throw new InvalidOperationException($"The cohort's ProviderId {cohort.ProviderId} doesn't match the expected ProviderId {command.ProviderId}");
            }

            if (cohort.Apprenticeships.Count != command.NumberOfApprentices)
            {
                throw new InvalidOperationException($"The number of apprentices in the cohort ({cohort.Apprenticeships.Count}) doesn't match the expected number ({command.NumberOfApprentices}");
            }

            return new GetDraftApprenticeshipCreatedEventsForCohortQueryResult(cohort.Apprenticeships.Select(x =>
                new DraftApprenticeshipCreatedEvent(x.Id, command.CohortId, x.Uln, x.ReservationId,
                    command.UploadedOn)));
        }
    }
}