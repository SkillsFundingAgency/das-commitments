﻿using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Models;

namespace SFA.DAS.CommitmentsV2.Application.Commands.AddHistory
{
    public class AddHistoryCommandHandler : AsyncRequestHandler<AddHistoryCommand>
    {
        private readonly ProviderCommitmentsDbContext _dbContext;

        public AddHistoryCommandHandler(ProviderCommitmentsDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task Handle(AddHistoryCommand request, CancellationToken cancellationToken)
        {
            var history = new History
            {
                EntityId = request.EntityId,
                CommitmentId = request.EntityType == nameof(Cohort) ? request.EntityId : default(long?),
                ApprenticeshipId = (request.EntityType == nameof(DraftApprenticeship) || request.EntityType == nameof(ApprovedApprenticeship)) ? request.EntityId : default(long?),
                OriginalState = request.InitialState,
                UpdatedState = request.UpdatedState,
                ChangeType = request.StateChangeType.ToString(),
                CreatedOn = request.UpdatedOn,
                UserId = request.UpdatingUserId,
                UpdatedByName = request.UpdatingUserName,
                UpdatedByRole = request.UpdatingParty.ToString(),
                EmployerAccountId = request.EmployerAccountId,
                ProviderId = request.ProviderId,
                EntityType =  request.EntityType,
                Diff = request.Diff,
                CorrelationId = request.CorrelationId
            };
            
            _dbContext.History.Add(history);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
