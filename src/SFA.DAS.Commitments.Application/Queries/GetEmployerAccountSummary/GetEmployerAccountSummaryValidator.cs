using System;
using FluentValidation;

namespace SFA.DAS.Commitments.Application.Queries.GetEmployerAccountSummary
{
    public sealed class GetEmployerAccountSummaryValidator : AbstractValidator<GetEmployerAccountSummaryRequest>
    {
        public GetEmployerAccountSummaryValidator()
        {
            RuleFor(request => request.Caller.Id).GreaterThan(0);
        }
    }
}
