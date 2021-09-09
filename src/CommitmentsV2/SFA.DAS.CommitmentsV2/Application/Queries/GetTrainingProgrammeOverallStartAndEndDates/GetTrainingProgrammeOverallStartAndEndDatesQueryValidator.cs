using FluentValidation;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetCalculatedTrainingProgrammeVersion
{
    public class GetTrainingProgrammeOverallStartAndEndDatesQueryValidator : AbstractValidator<GetTrainingProgrammeOverallStartAndEndDatesQuery>
    {
        public GetTrainingProgrammeOverallStartAndEndDatesQueryValidator()
        {
            RuleFor(q => q.CourseCode).NotEmpty().WithMessage("The course code must be supplied");
        }
    }
}
