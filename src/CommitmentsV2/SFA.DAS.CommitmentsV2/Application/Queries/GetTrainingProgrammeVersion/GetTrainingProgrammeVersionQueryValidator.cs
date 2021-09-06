using FluentValidation;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetTrainingProgrammeVersion
{
    public class GetTrainingProgrammeVersionQueryValidator : AbstractValidator<GetTrainingProgrammeVersionQuery>
    {
        public GetTrainingProgrammeVersionQueryValidator()
        {
            When(q => q.StandardUId == null, () =>
            {
                RuleFor(q => q.CourseCode).NotEmpty();
                RuleFor(q => q.Version).NotEmpty();
            });

            When(q => q.StandardUId == null && q.Version == null, () =>
            {
                RuleFor(q => q.StandardUId).NotEmpty();
            });
        }
    }
}
