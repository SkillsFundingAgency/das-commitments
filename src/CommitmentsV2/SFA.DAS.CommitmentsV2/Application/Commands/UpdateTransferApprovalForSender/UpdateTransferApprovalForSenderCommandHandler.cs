using MediatR;
using Microsoft.EntityFrameworkCore;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using SFA.DAS.CommitmentsV2.Types;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.Application.Commands.UpdateTransferApprovalForSender
{
    public class UpdateTransferApprovalForSenderCommandHandler : IRequestHandler<UpdateTransferApprovalForSenderCommand>
    {
        private readonly ITransferRequestDomainService _transferRequestDomainService;
        private readonly Lazy<ProviderCommitmentsDbContext> _dbContext;
        private readonly ICurrentDateTime _currentDateTime;

        public UpdateTransferApprovalForSenderCommandHandler(Lazy<ProviderCommitmentsDbContext> dbContext, ITransferRequestDomainService transferRequestDomainService,
            ICurrentDateTime currentDateTime)
        {
            _dbContext = dbContext;
            _transferRequestDomainService = transferRequestDomainService;
            _currentDateTime = currentDateTime;
        }

        public async Task Handle(UpdateTransferApprovalForSenderCommand command, CancellationToken cancellationToken)
        {
            var cohort = await _dbContext.Value.Cohorts
                .SingleOrDefaultAsync(c => c.Id == command.CohortId, cancellationToken);

            if(cohort == null)
            {
                throw new Exception($"Cannot find cohort: {command.CohortId}");
            }

            CheckAuthorization(command, cohort);
            CheckCommitmentStatus(command, cohort);

            if (command.TransferApprovalStatus == TransferApprovalStatus.Approved)
            {
                await _transferRequestDomainService.ApproveTransferRequest(command.TransferRequestId, command.UserInfo, _currentDateTime.UtcNow, cancellationToken);
            }
            else if(command.TransferApprovalStatus == TransferApprovalStatus.Rejected)
            {
                await _transferRequestDomainService.RejectTransferRequest(command.TransferRequestId, command.UserInfo, _currentDateTime.UtcNow, cancellationToken);
            }
        }

        private static void CheckAuthorization(UpdateTransferApprovalForSenderCommand command, Cohort cohort)
        {
            if (cohort.TransferSenderId != command.TransferSenderId)
                throw new Exception(
                    $"Employer {command.TransferSenderId} not authorised to access cohort: {command.CohortId} as transfer sender, expected transfer sender {cohort.TransferSenderId}");
        }

        private static void CheckCommitmentStatus(UpdateTransferApprovalForSenderCommand command, Cohort cohort)
        {
            if (cohort.EmployerAccountId != command.TransferReceiverId)
                throw new InvalidOperationException($"Cohort {cohort.Id} has employer account Id {cohort.EmployerAccountId} which doesn't match command receiver Id {command.TransferReceiverId}");

            if (cohort.CommitmentStatus == CommitmentStatus.Deleted)
                throw new InvalidOperationException($"Cohort {cohort.Id} cannot be updated because status is {cohort.CommitmentStatus}");

            if (cohort.TransferApprovalStatus != TransferApprovalStatus.Pending)
                throw new InvalidOperationException($"Transfer Approval for Cohort {cohort.Id} cannot be set because the status is {cohort.TransferApprovalStatus}");

            if (cohort.EditStatus != EditStatus.Both)
                throw new InvalidOperationException($"Transfer Sender {cohort.TransferSenderId} not allowed to approve until both the provider and receiving employer have approved");
        }
    }
}