using FluentValidation;
using SFA.DAS.Commitments.Domain;

namespace SFA.DAS.Commitments.Application.Queries.GetTransferRequest
{
    public sealed class GetTransferRequestValidator : AbstractValidator<GetTransferRequestRequest>
    {
        public GetTransferRequestValidator()
        {
            RuleFor(request => request.TransferRequestId).GreaterThan(0);
            RuleFor(request => request.Caller).NotNull();
            RuleFor(request => request.Caller.Id).GreaterThan(0);

            Custom(request => 
            {
                if (request.Caller.CallerType != CallerType.TransferReceiver &&
                    request.Caller.CallerType != CallerType.TransferSender)
                {
                    return new FluentValidation.Results.ValidationFailure("CallerType", "Caller Type Can only be TransferSender or TransferReceiver");
                }
                return null;
            });
        }
    }
}
