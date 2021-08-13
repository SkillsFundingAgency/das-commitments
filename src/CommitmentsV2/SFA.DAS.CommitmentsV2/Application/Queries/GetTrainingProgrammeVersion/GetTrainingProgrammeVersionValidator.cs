using FluentValidation;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetTrainingProgrammeVersion
{
    public class GetTrainingProgrammeVersionValidator : AbstractValidator<GetTrainingProgrammeVersionQuery>
    {
        public GetTrainingProgrammeVersionValidator()
        {
            RuleFor(q => q.StandardUId).NotEmpty();
        }
    }
}
