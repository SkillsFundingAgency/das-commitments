using FluentValidation;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetCalculatedTrainingProgrammeVersion;

public class GetCalculatedTrainingProgrammeVersionQueryValidator : AbstractValidator<GetCalculatedTrainingProgrammeVersionQuery>
{
    public GetCalculatedTrainingProgrammeVersionQueryValidator()
    {
        RuleFor(q => q.CourseCode).NotNull().GreaterThan(0).WithMessage("The course code must be supplied");
        RuleFor(q => q.StartDate).NotNull().WithMessage("The start date must be supplied");
    }
}