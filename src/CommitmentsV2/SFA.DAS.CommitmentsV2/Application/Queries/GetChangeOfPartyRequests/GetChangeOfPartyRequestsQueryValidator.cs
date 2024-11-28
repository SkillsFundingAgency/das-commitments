using FluentValidation;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetChangeOfPartyRequests;

public class GetChangeOfPartyRequestsQueryValidator : AbstractValidator<GetChangeOfPartyRequestsQuery>
{
    public GetChangeOfPartyRequestsQueryValidator()
    {
        RuleFor(x => x.ApprenticeshipId).GreaterThan(0);
    }
}