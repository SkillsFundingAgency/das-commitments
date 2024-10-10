using FluentValidation;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetTrainingProgrammeVersions;

public class GetTrainingProgrammeVersionsQueryValidator : AbstractValidator<GetTrainingProgrammeVersionsQuery>
{
    public GetTrainingProgrammeVersionsQueryValidator()
    {
        RuleFor(q => q.Id).NotEmpty();
    }
}