using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Application.Commands.CocApprovals;
using SFA.DAS.CommitmentsV2.Domain.Exceptions;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;

namespace SFA.DAS.CommitmentsV2.Application.Commands.EditApprenticeship;

public class PostCocApprovalCommandHandler(
    Lazy<ProviderCommitmentsDbContext> dbContext,
    ICocApprovalService cocApprovalService,
    ILogger<PostCocApprovalCommandHandler> logger)
    : IRequestHandler<PostCocApprovalCommand, CocApprovalResult>
{
    public async Task<CocApprovalResult> Handle(PostCocApprovalCommand command, CancellationToken cancellationToken)
    {
        logger.LogInformation("=== COMMITMENTS API: PostCocApprovalCommandHandler.Handle called ===");

        if (command == null)
        {
            throw new ArgumentNullException(nameof(command));
        }

        var db = dbContext.Value;
        var existingApprovalRequests = db.ApprovalRequests.Where(r => r.LearningKey == command.LearningKey); // && r.Status == Domain.Entities.CocApprovalRequestStatus.Pending);

        if (existingApprovalRequests.Any())
        {
            throw new DomainException("LearningKey", "An approval request for this learning key already exists.");
        }
    


        //var apprenticeship = await dbContext.Value.GetApprenticeshipAggregate(command.EditApprenticeshipRequest.ApprenticeshipId, cancellationToken);


        //logger.LogInformation("ApprenticeshipId: {ApprenticeshipId}", command.ApprenticeshipId);

        //if (command?.EditApprenticeshipRequest == null)
        //{
        //    throw new InvalidOperationException("Edit apprenticeship request is null");
        //}

        //logger.LogInformation("Determined Party: {Party}", party);
        //logger.LogInformation("AuthenticationServiceType: {AuthServiceType}", authenticationService.AuthenticationServiceType);

        //var apprenticeship = await dbContext.Value.GetApprenticeshipAggregate(command.EditApprenticeshipRequest.ApprenticeshipId, cancellationToken);

        //await Validate(command, apprenticeship, party, cancellationToken);

        //CreateImmediateUpdate(command, party, apprenticeship);

        //var immediateUpdateCreated = await CreateIntermediateUpdate(command, party, apprenticeship);

        return new CocApprovalResult();
    }

}