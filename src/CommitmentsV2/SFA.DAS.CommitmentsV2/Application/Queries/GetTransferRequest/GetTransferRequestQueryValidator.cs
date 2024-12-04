using FluentValidation;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetTransferRequest;

public class GetTransferRequestQueryValidator : AbstractValidator<GetTransferRequestQuery>
{
    public GetTransferRequestQueryValidator()
    {
        RuleFor(request => request.EmployerAccountId).GreaterThan(0);
        RuleFor(request => request.TransferRequestId).GreaterThan(0);
    }
}