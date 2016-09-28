using System;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using NLog;
using SFA.DAS.Commitments.Api.Core;
using SFA.DAS.Commitments.Api.Types;
using SFA.DAS.Commitments.Application.Commands.CreateCommitment;
using SFA.DAS.Commitments.Application.Commands.UpdateApprenticeship;
using SFA.DAS.Commitments.Application.Queries.GetApprenticeship;
using SFA.DAS.Commitments.Application.Queries.GetCommitment;
using SFA.DAS.Commitments.Application.Queries.GetEmployerCommitments;

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

        public async Task<OrchestratorResponse<GetEmployerCommitmentsResponse>> GetCommitments(long id)
        {
            try
            {
                var data = await _mediator.SendAsync(new GetEmployerCommitmentsRequest {AccountId = id});

                return new OrchestratorResponse<GetEmployerCommitmentsResponse>
                {
                    Data = data
                };
            }
            catch (ValidationException ex)
            {
                Logger.Info(ex, $"Validation error {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, ex.Message);
                throw;
            }
        }

        public async Task<OrchestratorResponse<GetCommitmentResponse>> GetCommitment(long accountId, long commitmentId)
        {
            try
            {
                var data = await _mediator.SendAsync(new GetCommitmentRequest
                {
                    AccountId = accountId,
                    CommitmentId = commitmentId
                });

                return new OrchestratorResponse<GetCommitmentResponse>
                {
                    Data = data
                };
            }
            catch (ValidationException ex)
            {
                Logger.Info(ex, $"Validation error {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, ex.Message);
                throw;
            }
        }

        public async Task<OrchestratorResponse<long>> CreateCommitment(long accountId, Commitment commitment)
        {
            try
            {
                var commitmentId = await _mediator.SendAsync(new CreateCommitmentCommand { Commitment = commitment });

                return new OrchestratorResponse<long>
                {
                    Data = commitmentId
                };
            }
            catch (ValidationException ex)
            {
                Logger.Info(ex, $"Validation error {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, ex.Message);
                throw;
            }
        }

        public async Task<OrchestratorResponse<GetApprenticeshipResponse>> GetApprenticeship(long accountId, long commitmentId, long apprenticeshipId)
        {
            try
            {
                var response = await _mediator.SendAsync(new GetApprenticeshipRequest
                {
                    AccountId = accountId,
                    CommitmentId = commitmentId,
                    ApprenticeshipId = apprenticeshipId
                });

                return new OrchestratorResponse<GetApprenticeshipResponse>
                {
                    Data = response
                };
            }
            catch (ValidationException ex)
            {
                Logger.Info(ex, $"Validation error {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, ex.Message);
                throw;
            }
        }

        public async Task<OrchestratorResponse> PutApprenticeship(long accountId, long commitmentId, long apprenticeshipId, Apprenticeship apprenticeship)
        {
            try
            {
                await _mediator.SendAsync(new UpdateApprenticeshipCommand
                {
                    AccountId = accountId,
                    CommitmentId = commitmentId,
                    ApprenticeshipId = apprenticeshipId,
                    Apprenticeship = apprenticeship
                });

                return new OrchestratorResponse();
            }
            catch (ValidationException ex)
            {
                Logger.Info(ex, $"Validation error {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, ex.Message);
                throw;
            }
        }
    }
}