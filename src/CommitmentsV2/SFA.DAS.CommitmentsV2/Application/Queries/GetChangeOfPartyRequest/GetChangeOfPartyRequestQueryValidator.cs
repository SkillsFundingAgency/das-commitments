using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetChangeOfPartyRequest
{
    public class GetChangeOfPartyRequestQueryValidator : AbstractValidator<GetChangeOfPartyRequestQuery>
    {
        public GetChangeOfPartyRequestQueryValidator()
        {
            RuleFor(x => x.ChangeOfPartyRequestId).GreaterThan(0);
        }
    }
}
