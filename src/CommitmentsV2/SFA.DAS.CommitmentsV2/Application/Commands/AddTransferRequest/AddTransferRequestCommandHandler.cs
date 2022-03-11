using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Data.Extensions;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Models.Api;

namespace SFA.DAS.CommitmentsV2.Application.Commands.AddTransferRequest
{
    public class AddTransferRequestCommandHandler : AsyncRequestHandler<AddTransferRequestCommand>
    {
        private readonly Lazy<ProviderCommitmentsDbContext> _dbContext;
        private readonly IFundingCapService _fundingCapService;
        private readonly ILogger<AddTransferRequestCommandHandler> _logger;
        private readonly IApprovalsOuterApiClient _apiClient;

        public AddTransferRequestCommandHandler(
            Lazy<ProviderCommitmentsDbContext> dbContext,
            IFundingCapService fundingCapService,
            ILogger<AddTransferRequestCommandHandler> logger,
            IApprovalsOuterApiClient apiClient)
        {
            _dbContext = dbContext;
            _fundingCapService = fundingCapService;
            _logger = logger;
            _apiClient = apiClient;
        }

        protected override async Task Handle(AddTransferRequestCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var db = _dbContext.Value;

                var cohort = await db.GetCohortAggregate(request.CohortId, cancellationToken: cancellationToken);

                var autoApproval = false;
                if (cohort.PledgeApplicationId.HasValue)
                {
                    var apiRequest = new GetPledgeApplicationRequest(cohort.PledgeApplicationId.Value);
                    var pledgeApplication = await _apiClient.Get<PledgeApplication>(apiRequest);
                    autoApproval = pledgeApplication.AutomaticApproval;
                }

                var fundingCapSummary = await _fundingCapService.FundingCourseSummary(cohort.Apprenticeships);

                cohort.AddTransferRequest(
                    JsonConvert.SerializeObject(fundingCapSummary.Select(x => new {x.CourseTitle, x.ApprenticeshipCount})),
                    fundingCapSummary.Sum(x => x.CappedCost), 
                    fundingCapSummary.Sum(x => x.ActualCap),
                    request.LastApprovedByParty,
                    autoApproval);

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