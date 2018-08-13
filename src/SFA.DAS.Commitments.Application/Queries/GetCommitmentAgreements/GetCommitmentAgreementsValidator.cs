using FluentValidation;
using SFA.DAS.Commitments.Domain;

namespace SFA.DAS.Commitments.Application.Queries.GetCommitmentAgreements
{
    public sealed class GetCommitmentAgreementsValidator : AbstractValidator<GetCommitmentAgreementsRequest>
    {
        public GetCommitmentAgreementsValidator()
        {
            Custom(request =>
            {
                switch (request.Caller.CallerType)
                {
                    case CallerType.Provider:
                        if (request.Caller.Id <= 0)
                            return new FluentValidation.Results.ValidationFailure("Id", "Id must be greater than zero.");
                        break;
                    default:
                        return new FluentValidation.Results.ValidationFailure("CallerType", "CallerType must be Provider.");
                }

                return null;
            });
        }
    }
}
