using System;
using System.Threading.Tasks;
using MediatR;
using NLog;
using SFA.DAS.Commitments.Api.Types;
using SFA.DAS.Commitments.Application.Commands.CreateApprenticeship;
using SFA.DAS.Commitments.Application.Commands.CreateCommitment;
using SFA.DAS.Commitments.Application.Commands.UpdateApprenticeship;
using SFA.DAS.Commitments.Application.Commands.UpdateApprenticeshipStatus;
using SFA.DAS.Commitments.Application.Commands.UpdateCommitmentAgreement;
using SFA.DAS.Commitments.Application.Queries.GetApprenticeship;
using SFA.DAS.Commitments.Application.Queries.GetCommitment;
using SFA.DAS.Commitments.Application.Queries.GetCommitments;
using SFA.DAS.Commitments.Domain;

namespace SFA.DAS.Commitments.Api.Orchestrators
{
    public class EmployerOrchestrator
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();
        private readonly IMediator _mediator;

        public EmployerOrchestrator(IMediator mediator)
        {
            if (mediator == null)
                throw new ArgumentNullException(nameof(mediator));
            _mediator = mediator;
        }

        public async Task<GetCommitmentsResponse> GetCommitments(long accountId)
        {
            Logger.Info($"Getting commitments for employer account {accountId}");

            return await _mediator.SendAsync(new GetCommitmentsRequest
            {
                Caller = new Caller
                {
                    CallerType = CallerType.Employer,
                    Id = accountId
                }
            });
        }

        public async Task<GetCommitmentResponse> GetCommitment(long accountId, long commitmentId)
        {
            Logger.Info($"Getting commitment {commitmentId} for employer account {accountId}");

            return await _mediator.SendAsync(new GetCommitmentRequest
            {
                Caller = new Caller
                {
                    CallerType = CallerType.Employer,
                    Id = accountId
                },
                CommitmentId = commitmentId
            });
        }

        public async Task<long> CreateCommitment(long accountId, Commitment commitment)
        {
            Logger.Info($"Creating commitment for employer account {accountId}");

            commitment.EmployerAccountId = accountId;

            return await _mediator.SendAsync(new CreateCommitmentCommand
            {
                Commitment = commitment
            });
        }

        public async Task<GetApprenticeshipResponse> GetApprenticeship(long accountId, long commitmentId, long apprenticeshipId)
        {
            Logger.Info($"Getting apprenticeship {apprenticeshipId} in commitment {commitmentId} for employer account {accountId}");

            return await _mediator.SendAsync(new GetApprenticeshipRequest
            {
                Caller = new Caller
                {
                    CallerType = CallerType.Employer,
                    Id = accountId
                },
                CommitmentId = commitmentId,
                ApprenticeshipId = apprenticeshipId
            });
        }

        public async Task<long> CreateApprenticeship(long accountId, long commitmentId, Apprenticeship apprenticeship)
        {
            Logger.Info($"Creating apprenticeship for commitment {commitmentId} for employer account {accountId}");

            apprenticeship.CommitmentId = commitmentId;

            return await _mediator.SendAsync(new CreateApprenticeshipCommand
            {
                Caller = new Caller
                {
                    CallerType = CallerType.Employer,
                    Id = accountId
                },
                CommitmentId = commitmentId,
                Apprenticeship = apprenticeship
            });
        }

        public async Task PutApprenticeship(long accountId, long commitmentId, long apprenticeshipId, Apprenticeship apprenticeship)
        {
            Logger.Info($"Updating apprenticeship {apprenticeshipId} in commitment {commitmentId} for employer account {accountId}");

            apprenticeship.CommitmentId = commitmentId;

            await _mediator.SendAsync(new UpdateApprenticeshipCommand
            {
                Caller = new Caller
                {
                    CallerType = CallerType.Employer,
                    Id = accountId
                },
                CommitmentId = commitmentId,
                ApprenticeshipId = apprenticeshipId,
                Apprenticeship = apprenticeship
            });
        }

        public async Task PatchCommitment(long accountId, long commitmentId, AgreementStatus agreementStatus)
        {
            Logger.Info($"Updating agreement status to {agreementStatus} for commitment {commitmentId} for employer account {accountId}");

            await _mediator.SendAsync(new UpdateCommitmentAgreementCommand
            {
                Caller = new Caller
                {
                    CallerType = CallerType.Employer,
                    Id = accountId
                },
                CommitmentId = commitmentId,
                AgreementStatus = agreementStatus
            });
        }

        public async Task PatchApprenticeship(long accountId, long commitmentId, long apprenticeshipId, PaymentStatus? paymentStatus)
        {
            Logger.Info($"Updating payment status to {paymentStatus} for commitment {commitmentId} for employer account {accountId}");

            await _mediator.SendAsync(new UpdateApprenticeshipStatusCommand
            {
                AccountId = accountId,
                CommitmentId = commitmentId,
                ApprenticeshipId = apprenticeshipId,
                PaymentStatus = paymentStatus
            });
        }
    }
}
