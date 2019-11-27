using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Data.QueryExtensions;
using SFA.DAS.CommitmentsV2.Exceptions;
using SFA.DAS.CommitmentsV2.Messages.Commands;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.CommandHandlers
{
    public class ProviderApproveCohortCommandHandler : IHandleMessages<ProviderApproveCohortCommand>
    {
        private readonly IDistributedCache _distributedCache;
        private readonly ILogger<ProviderApproveCohortCommandHandler> _logger;
        private readonly Lazy<ProviderCommitmentsDbContext> _dbContext;

        public ProviderApproveCohortCommandHandler(IDistributedCache distributedCache,
            ILogger<ProviderApproveCohortCommandHandler> logger,
            IMediator mediator, Lazy<ProviderCommitmentsDbContext> dbContext)
        {
            _distributedCache = distributedCache;
            _logger = logger;
            _dbContext = dbContext;
        }

        public async Task Handle(ProviderApproveCohortCommand message, IMessageHandlerContext context)
        {
            _logger.LogInformation($"Handled {nameof(ProviderApproveCohortCommand)} with MessageId '{context.MessageId}'");

            var cohort = await _dbContext.Value.Cohorts.Include(c => c.Apprenticeships).SingleOrDefaultAsync(c => c.Id == message.CohortId, new CancellationToken());
            //if (cohort == null) throw new BadRequestException($"Cohort {cohortId} was not found");
            //if (cohort.IsApprovedByAllParties) throw new InvalidOperationException($"Cohort {cohortId} is approved by all parties and can't be modified");

            cohort.Approve(Party.Provider, string.Empty, message.UserInfo, DateTime.UtcNow);

            await _dbContext.Value.SaveChangesAsync();

            await _distributedCache.SetStringAsync(context.MessageId, "OK");
        }
    }
}
