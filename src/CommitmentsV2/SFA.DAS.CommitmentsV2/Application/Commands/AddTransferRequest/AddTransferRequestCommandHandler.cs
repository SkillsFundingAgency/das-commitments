using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
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

                var fundingCapSummary = await _fundingCapService.FundingCourseSummary(cohort.Apprenticeships);

                cohort.AddTransferRequest(
                    JsonConvert.SerializeObject(fundingCapSummary.Select(x => new {x.CourseTitle, x.ApprenticeshipCount})),
                    fundingCapSummary.Sum(x => x.CappedCost), 
                    fundingCapSummary.Sum(x => x.ActualCap));

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