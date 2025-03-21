﻿using FluentValidation;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetProviderCommitmentAgreements;

public class GetProviderCommitmentAgreementValidator :  AbstractValidator<GetProviderCommitmentAgreementQuery>
{
    public GetProviderCommitmentAgreementValidator()
    {
        RuleFor(model => model.ProviderId).GreaterThan(0).WithMessage("Provider Id must be supplied");
    }
}