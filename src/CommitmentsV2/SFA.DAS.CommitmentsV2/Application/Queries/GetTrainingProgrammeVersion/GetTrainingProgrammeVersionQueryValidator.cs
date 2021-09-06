using FluentValidation;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetTrainingProgrammeVersion
{
    public class GetTrainingProgrammeVersionQueryValidator : AbstractValidator<GetTrainingProgrammeVersionQuery>
    {
        public GetTrainingProgrammeVersionQueryValidator()
        {
            When(q => string.IsNullOrEmpty(q.StandardUId), () =>
            {
                RuleFor(q => q.CourseCode).NotEmpty();
                RuleFor(q => q.Version).NotEmpty();
            });

            When(q => string.IsNullOrEmpty(q.CourseCode) && string.IsNullOrEmpty(q.Version), () =>
            {
                RuleFor(q => q.StandardUId).NotEmpty();
            });
        }
    }
}
