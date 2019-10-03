using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Data.Extensions;
using SFA.DAS.CommitmentsV2.Extensions;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.EAS.Account.Api.Client;
using SFA.DAS.NServiceBus.Services;

namespace SFA.DAS.CommitmentsV2.Application.Commands.ProcessFullyApprovedCohort
{
    public class ProcessFullyApprovedCohortCommandHandler : AsyncRequestHandler<ProcessFullyApprovedCohortCommand>
    {
        private readonly IAccountApiClient _accountApiClient;
        private readonly Lazy<ProviderCommitmentsDbContext> _db;
        private readonly IEventPublisher _eventPublisher;

        public ProcessFullyApprovedCohortCommandHandler(IAccountApiClient accountApiClient, Lazy<ProviderCommitmentsDbContext> db, IEventPublisher eventPublisher)
        {
            _accountApiClient = accountApiClient;
            _db = db;
            _eventPublisher = eventPublisher;
        }

        protected override async Task Handle(ProcessFullyApprovedCohortCommand request, CancellationToken cancellationToken)
        {
            var account = await _accountApiClient.GetAccount(request.AccountId);
            var apprenticeshipEmployerType = account.ApprenticeshipEmployerType.ToEnum<ApprenticeshipEmployerType>();
            
            await _db.Value.ProcessFullyApprovedCohort(request.CohortId, request.AccountId, apprenticeshipEmployerType);
            
            var events = await _db.Value.ApprovedApprenticeships
                .Where(a => a.Cohort.Id == request.CohortId)
                .Select(a => new ApprenticeshipCreatedEvent
                {
                    ApprenticeshipId = a.Id,
                    CreatedOn = a.Cohort.TransferApprovalActionedOn ?? a.AgreedOn.Value,
                    AgreedOn = a.AgreedOn.Value,
                    AccountId = a.Cohort.EmployerAccountId,
                    AccountLegalEntityPublicHashedId = a.Cohort.AccountLegalEntityPublicHashedId,
                    LegalEntityName = a.Cohort.LegalEntityName,
                    ProviderId = a.Cohort.ProviderId.Value,
                    TransferSenderId = a.Cohort.TransferSenderId,
                    ApprenticeshipEmployerTypeOnApproval = apprenticeshipEmployerType,
                    Uln = a.Uln,
                    TrainingType = a.ProgrammeType.Value,
                    TrainingCode = a.CourseCode,
                    StartDate = a.StartDate.Value,
                    EndDate = a.EndDate.Value,
                    PriceEpisodes = a.PriceHistory
                        .Select(p => new PriceEpisode
                        {
                            FromDate = p.FromDate,
                            ToDate = p.ToDate,
                            Cost = p.Cost
                        })
                        .ToArray()
                })
                .ToListAsync(cancellationToken);
            
            var tasks = events.Select(_eventPublisher.Publish);

            await Task.WhenAll(tasks);
        }
    }
}