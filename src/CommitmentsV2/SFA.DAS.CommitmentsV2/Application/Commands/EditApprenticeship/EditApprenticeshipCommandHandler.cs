using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Exceptions;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using SFA.DAS.CommitmentsV2.Data.Extensions;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Authentication;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.CommitmentsV2.Domain.Extensions;
using SFA.DAS.CommitmentsV2.Application.Queries.GetTrainingProgramme;
using SFA.DAS.CommitmentsV2.Application.Queries.GetTrainingProgrammeVersion;

namespace SFA.DAS.CommitmentsV2.Application.Commands.EditApprenticeship;

public class EditApprenticeshipCommandHandler(
    IEditApprenticeshipValidationService editValidationService,
    Lazy<ProviderCommitmentsDbContext> dbContext,
    IAuthenticationService authenticationService,
    ICurrentDateTime currentDateTime,
    IMediator mediator,
    ILogger<EditApprenticeshipCommandHandler> logger)
    : IRequestHandler<EditApprenticeshipCommand, EditApprenticeshipResponse>
{
    public async Task<EditApprenticeshipResponse> Handle(EditApprenticeshipCommand command, CancellationToken cancellationToken)
    {
        logger.LogInformation("=== COMMITMENTS API: EditApprenticeshipCommandHandler.Handle called ===");
        logger.LogInformation("ApprenticeshipId: {ApprenticeshipId}", command.EditApprenticeshipRequest?.ApprenticeshipId);
        
        if (command?.EditApprenticeshipRequest == null)
        {
            throw new InvalidOperationException("Edit apprenticeship request is null");
        }

        var party = GetParty(command);
        logger.LogInformation("Determined Party: {Party}", party);
        logger.LogInformation("AuthenticationServiceType: {AuthServiceType}", authenticationService.AuthenticationServiceType);

        var apprenticeship = await dbContext.Value.GetApprenticeshipAggregate(command.EditApprenticeshipRequest.ApprenticeshipId, cancellationToken);

        await Validate(command, apprenticeship, party, cancellationToken);

        CreateImmediateUpdate(command, party, apprenticeship);

        var immediateUpdateCreated = await CreateIntermediateUpdate(command, party, apprenticeship);

        return new EditApprenticeshipResponse { ApprenticeshipId = command.EditApprenticeshipRequest.ApprenticeshipId, NeedReapproval = immediateUpdateCreated };
    }

    private Party GetParty(EditApprenticeshipCommand command)
    {
        logger.LogInformation("=== COMMITMENTS API: GetParty called ===");
        
        if (command == null)
        {
            throw new InvalidOperationException("Command is null");
        }

        logger.LogInformation("Command.Party: {CommandParty}", command.Party);
        logger.LogInformation("AuthenticationServiceType: {AuthServiceType}", authenticationService.AuthenticationServiceType);

        Party result;
        
        if (command.Party != Party.None)
        {
            result = command.Party;
            logger.LogInformation("Using command.Party (explicitly set): {Party}", result);
        }
        else
        {
            if (authenticationService.AuthenticationServiceType == AuthenticationServiceType.MessageHandler)
            {
                result = command.Party;
                logger.LogInformation("Using command.Party (MessageHandler fallback): {Party}", result);
            }
            else
            {
                result = authenticationService.GetUserParty();
                logger.LogInformation("Using authenticationService.GetUserParty() (fallback): {Party}", result);
            }
        }

        logger.LogInformation("Final determined party: {Party}", result);
        return result;
    }

    private static void CreateImmediateUpdate(EditApprenticeshipCommand command, Party party, Apprenticeship apprenticeship)
    {
        if (command?.EditApprenticeshipRequest == null)
        {
            throw new InvalidOperationException("Edit apprenticeship request is null");
        }

        if (command.EmployerReferenceUpdateRequired(apprenticeship, party))
        {
            apprenticeship.UpdateEmployerReference(command.EditApprenticeshipRequest.EmployerReference, party, command.EditApprenticeshipRequest.UserInfo);
        }
        else if (command.ProviderReferenceUpdateRequired(apprenticeship, party))
        {
            apprenticeship.UpdateProviderReference(command.EditApprenticeshipRequest.ProviderReference, party, command.EditApprenticeshipRequest.UserInfo);
        }
    }

    private async Task<bool> CreateIntermediateUpdate(EditApprenticeshipCommand command, Party party, Apprenticeship apprenticeship)
    {
        if (command?.EditApprenticeshipRequest == null)
        {
            throw new InvalidOperationException("Edit apprenticeship request is null");
        }

        if (!command.EditApprenticeshipRequest.IntermediateApprenticeshipUpdateRequired(apprenticeship))
        {
            return false;
        }

        var apprenticeshipUpdate = command.MapToApprenticeshipUpdate(apprenticeship, party, currentDateTime.UtcNow);

        if (!string.IsNullOrWhiteSpace(apprenticeshipUpdate.TrainingCode) || !string.IsNullOrWhiteSpace(apprenticeshipUpdate.TrainingCourseVersion))
        {
            var version = command.EditApprenticeshipRequest.Version ?? apprenticeship.TrainingCourseVersion;
            var courseCode = command.EditApprenticeshipRequest.CourseCode ?? apprenticeship.CourseCode;

            if (int.TryParse(courseCode, out _))
            {
                var result = await mediator.Send(new GetTrainingProgrammeVersionQuery(courseCode, version));

                if (result == null)
                {
                    throw new InvalidOperationException("Invalid training programme");
                }

                var standardVersion = result.TrainingProgramme;

                apprenticeshipUpdate.TrainingName = apprenticeship.CourseName != standardVersion.Name ? standardVersion.Name : null;
                apprenticeshipUpdate.StandardUId = standardVersion.StandardUId;
                apprenticeshipUpdate.TrainingCourseVersion = standardVersion.Version;
                apprenticeshipUpdate.TrainingCourseVersionConfirmed = true;

                if (apprenticeship.ProgrammeType.Value != standardVersion.ProgrammeType)
                {
                    apprenticeshipUpdate.TrainingType = standardVersion.ProgrammeType;
                }
            }
            else
            {
                var result = await mediator.Send(new GetTrainingProgrammeQuery
                {
                    Id = apprenticeshipUpdate.TrainingCode
                });

                if (result == null || result.TrainingProgramme == null)
                {
                    throw new InvalidOperationException("Invalid training programme");
                }

                apprenticeshipUpdate.TrainingName = result.TrainingProgramme?.Name;
                apprenticeshipUpdate.TrainingType = result.TrainingProgramme?.ProgrammeType;
                apprenticeshipUpdate.TrainingCourseVersion = null;
                apprenticeshipUpdate.TrainingCourseVersionConfirmed = null;
                apprenticeshipUpdate.StandardUId = null;
            }
        }

        apprenticeship.CreateApprenticeshipUpdate(apprenticeshipUpdate, party);
        
        return true;
    }

    private async Task Validate(EditApprenticeshipCommand command, Apprenticeship apprenticeship, Party party, CancellationToken cancellationToken)
    {
        CheckAuthorisation(command, apprenticeship, party);

        var validationResult = await editValidationService.Validate(command.CreateValidationRequest(apprenticeship, currentDateTime.UtcNow), cancellationToken, party);
        if (validationResult?.Errors?.Count > 0)
        {
            // This shouldn't get triggered as these checks should already been passed.
            // But in case someone is calling the EditApprenticeship endpoint directly
            var messages = string.Empty;
            validationResult.Errors.ForEach(x => messages += x.PropertyName + ":" + x.ErrorMessage);
            logger.LogError("Invalid operation for edit - the following error/s occured : {Messages}", messages);
            throw new InvalidOperationException("The operation is not allowed for the current state of the object");
        }

        if (apprenticeship.ApprenticeshipUpdate.Any(a => a.Status == ApprenticeshipUpdateStatus.Pending))
        {
            throw new InvalidOperationException("Unable to create an ApprenticeshipUpdate for an Apprenticeship with a pending update");
        }

        if (apprenticeship.EmailAddressConfirmed == true && command.EditApprenticeshipRequest?.Email != null)
        {
            throw new DomainException("ConfirmChanges", "Unable to make these changes, as the apprentice has confirmed their email address");
        }
    }

    private static void CheckAuthorisation(EditApprenticeshipCommand command, Apprenticeship apprenticeship, Party party)
    {
        if (command?.EditApprenticeshipRequest == null)
        {
            throw new UnauthorizedAccessException("Edit apprenticeship request is null");
        }

        if (apprenticeship?.Cohort == null)
        {
            throw new UnauthorizedAccessException($"Cohort not found for apprenticeship {apprenticeship?.Id}");
        }

        if (command.EditApprenticeshipRequest.UserInfo == null)
        {
            throw new UnauthorizedAccessException("User information is required for authorization");
        }

        switch (party)
        {
            case Party.Employer:
                if (apprenticeship.Cohort.EmployerAccountId != command.EditApprenticeshipRequest.AccountId)
                    throw new UnauthorizedAccessException($"Employer {command.EditApprenticeshipRequest.UserInfo.UserId} not authorised to update apprenticeship {apprenticeship.Id}");
                break;
            case Party.Provider:
                if (apprenticeship.Cohort.ProviderId != command.EditApprenticeshipRequest.ProviderId)
                    throw new UnauthorizedAccessException($"ProviderId : {command.EditApprenticeshipRequest.ProviderId} - UserInfo : {command.EditApprenticeshipRequest.UserInfo.UserId} - not authorised to update apprenticeship {apprenticeship.Id}");
                break;

            default:
                throw new UnauthorizedAccessException($"Party {party} is not permitted to make changes");
        }
    }
}