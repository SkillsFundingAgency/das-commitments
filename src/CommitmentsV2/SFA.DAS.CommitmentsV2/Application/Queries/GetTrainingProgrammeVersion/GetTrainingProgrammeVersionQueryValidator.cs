using FluentValidation;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetTrainingProgrammeVersion
{
    public class GetTrainingProgrammeVersionQueryValidator : AbstractValidator<GetTrainingProgrammeVersionQuery>
    {
        public GetTrainingProgrammeVersionQueryValidator()
        {
            RuleFor(q => q.StandardUId).NotEmpty();
        }
    }
}
