using FluentValidation;

namespace SFA.DAS.Commitments.Application.Commands.ApproveTransferRequest
{
    public sealed class ApproveTransferRequestValidator : AbstractValidator<ApproveTransferRequestCommand>
    {
        public ApproveTransferRequestValidator()
        {
            RuleFor(x => x.TransferRequestId).GreaterThan(0);
            RuleFor(x => x.CommitmentId).GreaterThan(0);
            RuleFor(x => x.TransferSenderId).GreaterThan(0);
            RuleFor(x => x.TransferReceiverId).GreaterThan(0);
            RuleFor(x => x.UserEmail).NotNull();
            RuleFor(x => x.UserName).NotNull();
        }
    }
}
