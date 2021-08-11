using FluentValidation;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetTrainingProgrammeVersion
{
    public class GetTrainingProgrammeVersionQueryValidator : AbstractValidator<GetTrainingProgrammeVersionQuery>
    {
        public GetTrainingProgrammeVersionQueryValidator()
        {
            RuleFor(q => q.CourseCode).NotNull().GreaterThan(0).WithMessage("The course code must be supplied");
            RuleFor(q => q.StartDate).NotNull().WithMessage("The start date must be supplied");
        }
    }
}
