using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Models;

namespace SFA.DAS.CommitmentsV2.Application.Commands.AddHistory
{
    public class AddHistoryCommandHandler : IRequestHandler<AddHistoryCommand>
    {
        private readonly ProviderCommitmentsDbContext _dbContext;

        public AddHistoryCommandHandler(ProviderCommitmentsDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task Handle(AddHistoryCommand request, CancellationToken cancellationToken)
        {
            var history = new History
            {
                EntityId = request.EntityId,
                CommitmentId = request.EntityType == nameof(Cohort) ? request.EntityId : default(long?),
                ApprenticeshipId = GetApprenticeshipId(request),
                OriginalState = request.InitialState,
                UpdatedState = request.UpdatedState,
                ChangeType = request.StateChangeType.ToString(),
                CreatedOn = request.UpdatedOn,
                UserId = request.UpdatingUserId,
                UpdatedByName = request.UpdatingUserName,
                UpdatedByRole = request.UpdatingParty.ToString(),
                EmployerAccountId = request.EmployerAccountId,
                ProviderId = request.ProviderId,
                EntityType = request.EntityType,
                Diff = request.Diff,
                CorrelationId = request.CorrelationId
            };

            _dbContext.History.Add(history);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        private long? GetApprenticeshipId(AddHistoryCommand request)
        {
            if (request.ApprenticeshipId.HasValue)
                return request.ApprenticeshipId;

            if (request.EntityType == nameof(DraftApprenticeship) || request.EntityType == nameof(Apprenticeship))
                return request.EntityId;

            return default(long?);
        }

    }
}
