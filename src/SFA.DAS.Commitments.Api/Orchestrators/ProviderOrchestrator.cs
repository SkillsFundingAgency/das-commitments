using System;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using NLog;
using SFA.DAS.Commitments.Api.Core;
using SFA.DAS.Commitments.Application.Commands.CreateApprenticeship;
using SFA.DAS.Commitments.Application.Commands.UpdateApprenticeship;
using SFA.DAS.Commitments.Application.Commands.UpdateCommitmentStatus;
using SFA.DAS.Commitments.Application.Queries.GetApprenticeship;
using SFA.DAS.Commitments.Application.Queries.GetCommitment;
using SFA.DAS.Commitments.Application.Queries.GetCommitments;
using SFA.DAS.Commitments.Domain;
using Apprenticeship = SFA.DAS.Commitments.Api.Types.Apprenticeship;
using CommitmentStatus = SFA.DAS.Commitments.Api.Types.CommitmentStatus;

namespace SFA.DAS.Commitments.Api.Orchestrators
{
    public class ProviderOrchestrator
    {
        private readonly IMediator _mediator;
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        public ProviderOrchestrator(IMediator mediator)
        {
            if (mediator == null)
                throw new ArgumentNullException(nameof(mediator));
            _mediator = mediator;
        }

        public async Task<OrchestratorResponse<GetCommitmentsResponse>> GetCommitments(long id)
        {
            try
            {
                var response = await _mediator.SendAsync(new GetCommitmentsRequest
                {
                    Caller = new Caller
                    {
                        CallerType = CallerType.Provider,
                        Id = id
                    }
                });

                return new OrchestratorResponse<GetCommitmentsResponse>
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

        public async Task<OrchestratorResponse<GetCommitmentResponse>> GetCommitment(long providerId, long commitmentId)
        {
            try
            {
                var response = await _mediator.SendAsync(new GetCommitmentRequest
                {
                    Caller = new Caller
                    {
                        CallerType = CallerType.Provider,
                        Id = providerId
                    },
                    CommitmentId = commitmentId
                });

                return new OrchestratorResponse<GetCommitmentResponse>
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

        public async Task<OrchestratorResponse<GetApprenticeshipResponse>> GetApprenticeship(long providerId, long commitmentId, long apprenticeshipId)
        {
            try
            {
                var response = await _mediator.SendAsync(new GetApprenticeshipRequest
                {
                    Caller = new Caller
                    {
                        CallerType = CallerType.Provider,
                        Id = providerId
                    },
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

        public async Task<OrchestratorResponse<long>> CreateApprenticeship(long providerId, long commitmentId, Apprenticeship apprenticeship)
        {
            try
            {
                var apprenticeshipId = await _mediator.SendAsync(new CreateApprenticeshipCommand
                {
                    ProviderId = providerId,
                    CommitmentId = commitmentId,
                    Apprenticeship = apprenticeship
                });

                return new OrchestratorResponse<long>
                {
                    Data = apprenticeshipId
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

        public async Task<OrchestratorResponse> PutApprenticeship(long providerId, long commitmentId, long apprenticeshipId, Apprenticeship apprenticeship)
        {
            try
            {
                await _mediator.SendAsync(new UpdateApprenticeshipCommand
                {
                    ProviderId = providerId,
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

        public async Task<OrchestratorResponse> PatchCommitment(long providerId, long commitmentId, CommitmentStatus? status)
        {
            try
            {
                await _mediator.SendAsync(new UpdateCommitmentStatusCommand
                {
                    AccountId = providerId,
                    CommitmentId = commitmentId,
                    Status = status
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