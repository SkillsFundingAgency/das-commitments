using FluentValidation;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetPriceEpisodes
{
    public class GetPriceEpisodesQueryValidator : AbstractValidator<GetPriceEpisodesQuery>
    {
        public GetPriceEpisodesQueryValidator()
        {
            RuleFor(q => q.ApprenticeshipId).GreaterThan(0);
        }
    }
}
