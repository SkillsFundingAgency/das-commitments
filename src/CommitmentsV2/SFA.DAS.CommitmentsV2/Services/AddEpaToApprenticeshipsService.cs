using MediatR;
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Application.Commands.AddLastSubmissionEventId;
using SFA.DAS.CommitmentsV2.Application.Commands.UpdateApprenticeshipsWithEpaOrgId;
using SFA.DAS.CommitmentsV2.Application.Commands.UpdateCacheOfAssessmentOrganisations;
using SFA.DAS.CommitmentsV2.Application.Queries.GetLastSubmissionEventId;
using SFA.DAS.CommitmentsV2.Application.Queries.GetSubmissionEvents;
using SFA.DAS.CommitmentsV2.Domain.Interfaces.AddEpaToApprenticeship;
using SFA.DAS.CommitmentsV2.Models.ApprovalsOuterApi.Types;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.Services
{
    public class AddEpaToApprenticeshipsService : IAddEpaToApprenticeshipService
    {
        private readonly IMediator _mediator;
        private readonly ILogger<AddEpaToApprenticeshipsService> _logger;

        public AddEpaToApprenticeshipsService(IMediator mediator, ILogger<AddEpaToApprenticeshipsService> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        public async Task Update()
        {
            var lastId = await _mediator.Send(new GetLastSubmissionEventIdQuery());
            await _mediator.Send(new UpdateCacheOfAssessmentOrganisationsCommand());

            PageOfResults<SubmissionEvent> page;
            long? pageLastId;
            do
            {
                page =  await _mediator.Send(new GetSubmissionEventsQuery(lastId));
                if (page == null || page.Items == null || !page.Items.Any())
                {
                    _logger.LogInformation("No SubmissionEvents to process");
                    return;
                }
                _logger.LogInformation($"Retrieved {page.Items.Length} SubmissionEvents");

                pageLastId = await _mediator.Send(new UpdateApprenticeshipsWithEpaOrgIdCommand(page.Items));
                if (pageLastId != null)
                {
                    _logger.LogInformation($"Storing latest SubmissionEventId as {pageLastId.Value}");
                    await _mediator.Send(new AddLastSubmissionEventIdCommand(pageLastId.Value));
                    lastId = pageLastId.Value;
                }
            } while (pageLastId.HasValue && page.TotalNumberOfPages > page.PageNumber);

        }
    }
}
