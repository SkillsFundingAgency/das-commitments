using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Application.Commands.CocApprovals;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using SFA.DAS.CommitmentsV2.Extensions;
using FluentValidation;
using SFA.DAS.CommitmentsV2.Domain.Exceptions;
using SFA.DAS.CommitmentsV2.Data;
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Data.Extensions;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Exceptions;

namespace SFA.DAS.CommitmentsV2.Mapping.CocApprovals;

public class CocApprovalRequestToCocApprovalCommandMapper(
    Lazy<ProviderCommitmentsDbContext> dbContext,
    ILogger<CocApprovalRequestToCocApprovalCommandMapper> logger) : IMapper<CocApprovalRequest, PostCocApprovalCommand>
{
    public async Task<PostCocApprovalCommand> Map(CocApprovalRequest request)
    {
        var result = new PostCocApprovalCommand
        {
            LearningKey = request.LearningKey,
            ApprenticeshipId = request.ApprenticeshipId,
            LearningType = EnumExtensions.FromDescription<CocLearningType>(request.LearningType),
            ProviderId = ToLong(request.UKPRN),
            ULN = request.ULN,
            Changes = new CocChanges(),
            ApprovalFieldChanges = request.Changes,
            Apprenticeship = await GetApprenticeship(request.ApprenticeshipId)
        };

        foreach (var change in request.Changes)
        {
            var changeType = EnumExtensions.FromDescription<CocChangeField>(change.ChangeType);

            if(changeType == CocChangeField.TNP1 || changeType == CocChangeField.TNP2)
            {
                var update = new CocUpdate<int>
                {
                    Old = ToInt(change.Data.Old),
                    New = ToInt(change.Data.New)
                };

                switch (changeType) 
                { 
                    case CocChangeField.TNP1: 
                        result.Changes.TNP1 = update; 
                        break; 
                    case CocChangeField.TNP2: 
                        result.Changes.TNP2 = update; 
                        break; 
                }
            }
        }
        return result;
    }

    public async Task<Apprenticeship> GetApprenticeship(long id)
    {
        try
        {
            return await dbContext.Value.GetApprenticeshipAggregate(id, CancellationToken.None);
        }
        catch (BadRequestException ex)
        {
            logger.LogError(ex, "ApprenticeshipId {ApprenticeshipId} not found, set it to null", id);
            return null;
        }
    }

    public static int ToInt(string value)
    {
        if (int.TryParse(value, out var result))
        {
            return result;
        }
        throw new DomainException("Data", "String could not be converted to an integer");
    }

    public static long ToLong(string value)
    {
        if (long.TryParse(value, out var result))
        {
            return result;
        }
        throw new DomainException("Data", "String could not be converted to an integer");
    }

}