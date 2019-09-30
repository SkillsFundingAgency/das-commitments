using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SFA.DAS.Apprenticeships.Api.Client;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;

namespace SFA.DAS.CommitmentsV2.Application.Commands.AddTransferRequest
{
    public class AddTransferRequestCommandHandler : AsyncRequestHandler<AddTransferRequestCommand>
    {
        private readonly Lazy<ProviderCommitmentsDbContext> _dbContext;
        private readonly IFundingCapService _fundingCapService;
        private readonly ILogger<AddTransferRequestCommandHandler> _logger;

        public AddTransferRequestCommandHandler(
            Lazy<ProviderCommitmentsDbContext> dbContext,
            IFundingCapService fundingCapService,
            ILogger<AddTransferRequestCommandHandler> logger)
        {
            _dbContext = dbContext;
            _fundingCapService = fundingCapService;
            _logger = logger;
        }

        protected override async Task Handle(AddTransferRequestCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var db = _dbContext.Value;

                var cohort = await db.Cohorts
                    .Include(c => c.Apprenticeships)
                    .Include(c => c.TransferRequests)
                    .SingleAsync(c => c.Id == request.CohortId, cancellationToken: cancellationToken);


                //var fundingCaps = await _fundingCapService.GetFundingCapsFor(cohort.Apprenticeships);

                cohort.AddTransferRequest("[]", 1000, 1100);
                await db.SaveChangesAsync(cancellationToken);
            }
            catch (Exception e)
            {
                _logger.LogError("Error Adding Transfer Request", e);
                throw;
            }
        }
    }
}