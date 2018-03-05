using FluentValidation;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Entities;

namespace SFA.DAS.Commitments.Application.Commands.TransferApproval
{
    public sealed class TransferApprovalValidator : AbstractValidator<TransferApprovalCommand>
    {
        public TransferApprovalValidator()
        {
            RuleFor(x => x.CommitmentId).GreaterThan(0);
            RuleFor(x => x.TransferSenderId).GreaterThan(0);
            RuleFor(x => x.UserEmail).NotNull();
            RuleFor(x => x.UserName).NotNull();
            RuleFor(x => x.TransferStatus).NotEqual(TransferApprovalStatus.Pending);
        }
    }
}
