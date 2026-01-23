using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Application.Commands.CocApprovals;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using SFA.DAS.CommitmentsV2.Extensions;
using FluentValidation;
using SFA.DAS.CommitmentsV2.Domain.Exceptions;

namespace SFA.DAS.CommitmentsV2.Mapping.CocApprovals;

public class CocApprovalRequestToCocApprovalCommandMapper : IMapper<CocApprovalRequest, PostCocApprovalCommand>
{
    public Task<PostCocApprovalCommand> Map(CocApprovalRequest request)
    {
        var result = new PostCocApprovalCommand
        {
            LearningKey = request.LearningKey,
            ApprenticeshipId = request.ApprenticeshipId,
            LearningType = EnumExtensions.FromDescription<CocLearningType>(request.LearningType),
            UKPRN = request.UKPRN,
            ULN = request.ULN,
            Changes = new CocChanges(),
            ApprovalFieldChanges = request.Changes
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
        return Task.FromResult(result);
    }

    public static int ToInt(string value)
    {
        if (int.TryParse(value, out var result))
        {
            return result;
        }
        throw new DomainException("Data", "String could not be converted to an integer");
    } 
}