using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using SFA.DAS.Commitments.Application.Exceptions;
using SFA.DAS.Commitments.Application.Queries.GetOverlappingApprenticeships;
using SFA.DAS.Commitments.Application.Rules;
using SFA.DAS.Commitments.Application.Services;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Entities.Validation;

namespace SFA.DAS.Commitments.Application.Commands.EmployerApproveCohort
{
    public sealed class EmployerApproveCohortCommandHandler : AsyncRequestHandler<EmployerApproveCohortCommand>
    {
        private readonly AbstractValidator<EmployerApproveCohortCommand> _validator;
        private readonly ICommitmentRepository _commitmentRepository;
        private readonly IApprenticeshipRepository _apprenticeshipRepository;
        private readonly OverlappingApprenticeshipService _overlappingApprenticeshipService;

        public EmployerApproveCohortCommandHandler(AbstractValidator<EmployerApproveCohortCommand> validator, ICommitmentRepository commitmentRepository, IApprenticeshipRepository apprenticeshipRepository, IApprenticeshipOverlapRules overlapRules)
        {
            _validator = validator;
            _commitmentRepository = commitmentRepository;
            _apprenticeshipRepository = apprenticeshipRepository;
            _overlappingApprenticeshipService = new OverlappingApprenticeshipService(apprenticeshipRepository, overlapRules);
        }

        protected override async Task HandleCore(EmployerApproveCohortCommand message)
        {
            _validator.ValidateAndThrow(message);

            var commitment = await GetCommitment(message.CommitmentId);
            await CheckCommitmentCanBeApproved(commitment, message.Caller.Id);
            await SetApprenticeshipStatuses(commitment);
        }

        private async Task SetApprenticeshipStatuses(Commitment commitment)
        {
            var newAgreementStatus = DetermineNewAgreementStatus(commitment);
            var newPaymentStatus = DetermineNewPaymentStatus(newAgreementStatus);
            commitment.Apprenticeships.ForEach(x =>
            {
                x.AgreementStatus = newAgreementStatus;
                x.PaymentStatus = newPaymentStatus;
            });
            await _apprenticeshipRepository.UpdateApprenticeshipStatuses(commitment.Apprenticeships);
        }

        private static PaymentStatus DetermineNewPaymentStatus(AgreementStatus newAgreementStatus)
        {
            return newAgreementStatus == AgreementStatus.BothAgreed ? PaymentStatus.Active : PaymentStatus.PendingApproval;
        }

        private static AgreementStatus DetermineNewAgreementStatus(Commitment commitment)
        {
            var currentAgreementStatus = GetCurrentAgreementStatus(commitment);
            var newAgreementStatus = currentAgreementStatus == AgreementStatus.ProviderAgreed ? AgreementStatus.BothAgreed : AgreementStatus.EmployerAgreed;
            return newAgreementStatus;
        }

        private static AgreementStatus GetCurrentAgreementStatus(Commitment commitment)
        {
            // Comment 1: This assumes, correctly, that during approval all apprenticeships have the same status.
            // Comment 2: I hate comments.
            return commitment.Apprenticeships.First().AgreementStatus;
        }

        private async Task CheckCommitmentCanBeApproved(Commitment commitment, long callerEmployerAccountId)
        {
            CheckCommitmentStatus(commitment);
            CheckEditStatus(commitment);
            CheckAuthorization(callerEmployerAccountId, commitment);
            CheckStateForApproval(commitment);
            await CheckOverlaps(commitment);
        }

        private async Task CheckOverlaps(Commitment commitment)
        {
            if (await _overlappingApprenticeshipService.CommitmentHasOverlappingApprenticeships(commitment))
            {
                throw new ValidationException("Unable to approve commitment with overlapping apprenticeships");
            }
        }

        private async Task<Commitment> GetCommitment(long commitmentId)
        {
            var commitment = await _commitmentRepository.GetCommitmentById(commitmentId);
            return commitment;
        }

        private static void CheckCommitmentStatus(Commitment commitment)
        {
            if (commitment.CommitmentStatus == CommitmentStatus.Deleted)
            {
                throw new InvalidOperationException($"Commitment {commitment.Id} cannot be updated because status is {commitment.CommitmentStatus}");
            }
        }

        private static void CheckEditStatus(Commitment commitment)
        {
            if (commitment.EditStatus != EditStatus.Both && commitment.EditStatus != EditStatus.EmployerOnly)
            {
                throw new UnauthorizedException($"Employer not allowed to edit commitment: {commitment.Id}");
            }
        }

        private static void CheckAuthorization(long employerAccountId, Commitment commitment)
        {
            if (commitment.EmployerAccountId != employerAccountId)
            {
                throw new UnauthorizedException($"Employer {employerAccountId} not authorised to access commitment: {commitment.Id}, expected employer {commitment.EmployerAccountId}");
            }
        }

        private static void CheckStateForApproval(Commitment commitment)
        {
            if (!commitment.EmployerCanApproveCommitment)
            {
                throw new InvalidOperationException($"Commitment {commitment.Id} cannot be approved because apprentice information is incomplete");
            }
        }
    }
}
