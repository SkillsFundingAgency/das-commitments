using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Application.Commands.AddLastSubmissionEventId;
using SFA.DAS.CommitmentsV2.Application.Commands.UpdateApprenticeshipsWithEpaOrgId;
using SFA.DAS.CommitmentsV2.Application.Commands.UpdateCacheOfAssessmentOrganisations;
using SFA.DAS.CommitmentsV2.Application.Queries.GetLastSubmissionEventId;
using SFA.DAS.CommitmentsV2.Application.Queries.GetSubmissionEvents;
using SFA.DAS.CommitmentsV2.Domain.Interfaces.AddEpaToApprenticeship;
using SFA.DAS.CommitmentsV2.Models.ApprovalsOuterApi.Types;

namespace SFA.DAS.CommitmentsV2.Services;

public class AddEpaToApprenticeshipsService(IMediator mediator, ILogger<AddEpaToApprenticeshipsService> logger)
    : IAddEpaToApprenticeshipService
{
    public async Task Update()
    {
        var lastId = await mediator.Send(new GetLastSubmissionEventIdQuery());
        await mediator.Send(new UpdateCacheOfAssessmentOrganisationsCommand());

        PageOfResults<SubmissionEvent> page;
        long? pageLastId;
        
        do
        {
            page =  await mediator.Send(new GetSubmissionEventsQuery(lastId));
            
            if (page == null || page.Items == null || !page.Items.Any())
            {
                logger.LogInformation("No SubmissionEvents to process");
                return;
            }
            
            logger.LogInformation("Retrieved {Length} SubmissionEvents", page.Items.Length);

            pageLastId = await mediator.Send(new UpdateApprenticeshipsWithEpaOrgIdCommand(page.Items));
            
            if (pageLastId != null)
            {
                logger.LogInformation("Storing latest SubmissionEventId as {LastId}", pageLastId.Value);
            
                await mediator.Send(new AddLastSubmissionEventIdCommand(pageLastId.Value));
                
                lastId = pageLastId.Value;
            }
            
        } while (pageLastId.HasValue && page.TotalNumberOfPages > page.PageNumber);
    }
}