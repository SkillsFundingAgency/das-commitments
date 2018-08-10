using FluentValidation;
using SFA.DAS.Commitments.Domain;

namespace SFA.DAS.Commitments.Application.Queries.GetCommitmentAgreements
{
    public sealed class GetCommitmentAgreementsValidator : AbstractValidator<GetCommitmentAgreementsRequest>
    {
        public GetCommitmentAgreementsValidator()
        {
            //todo: do we want to validade that the callertype is provider?
            Custom(request =>
            {
                switch (request.Caller.CallerType)
                {
                    case CallerType.Provider:
                        if (request.Caller.Id <= 0)
                            return new FluentValidation.Results.ValidationFailure("ProviderId", "ProviderId must be greater than zero.");
                        break;
                    //case CallerType.Employer:
                    ////default:
                    //    if (request.Caller.Id <= 0)
                    //        return new FluentValidation.Results.ValidationFailure("AccountId", "AccountId must be greater than zero.");
                    //    break;
                }

                return null;
            });
        }
    }
}
