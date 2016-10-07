using System;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using NLog;
using SFA.DAS.Commitments.Application.Commands.CreateApprenticeship;
using SFA.DAS.Commitments.Application.Commands.UpdateApprenticeship;
using SFA.DAS.Commitments.Application.Commands.UpdateCommitmentStatus;
using SFA.DAS.Commitments.Application.Exceptions;
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

        public async Task<GetCommitmentsResponse> GetCommitments(long id)
        {
            try
            {
                return await _mediator.SendAsync(new GetCommitmentsRequest
                {
                    Caller = new Caller
                    {
                        CallerType = CallerType.Provider,
                        Id = id
                    }
                });
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

        public async Task<GetCommitmentResponse> GetCommitment(long providerId, long commitmentId)
        {
            try
            {
                return await _mediator.SendAsync(new GetCommitmentRequest
                {
                    Caller = new Caller
                    {
                        CallerType = CallerType.Provider,
                        Id = providerId
                    },
                    CommitmentId = commitmentId
                });
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

        public async Task<GetApprenticeshipResponse> GetApprenticeship(long providerId, long commitmentId, long apprenticeshipId)
        {
            try
            {
                return await _mediator.SendAsync(new GetApprenticeshipRequest
                {
                    Caller = new Caller
                    {
                        CallerType = CallerType.Provider,
                        Id = providerId
                    },
                    CommitmentId = commitmentId,
                    ApprenticeshipId = apprenticeshipId
                });
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

        public async Task<long> CreateApprenticeship(long providerId, long commitmentId, Apprenticeship apprenticeship)
        {
            try
            {
                return await _mediator.SendAsync(new CreateApprenticeshipCommand
                {
                    Caller = new Caller
                    {
                        CallerType = CallerType.Provider,
                        Id = providerId
                    },
                    CommitmentId = commitmentId,
                    Apprenticeship = apprenticeship
                });
            }
            catch (ValidationException ex)
            {
                Logger.Info(ex, $"Validation error {ex.Message}");
                throw;
            }
            catch (UnauthorizedException ex)
            {
                Logger.Info(ex, $"Unauthorized error {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, ex.Message);
                throw;
            }
        }

        public async Task PutApprenticeship(long providerId, long commitmentId, long apprenticeshipId, Apprenticeship apprenticeship)
        {
            try
            {
                await _mediator.SendAsync(new UpdateApprenticeshipCommand
                {
                    Caller = new Caller
                    {
                        CallerType = CallerType.Provider,
                        Id = providerId
                    },
                    CommitmentId = commitmentId,
                    ApprenticeshipId = apprenticeshipId,
                    Apprenticeship = apprenticeship
                });
            }
            catch (ValidationException ex)
            {
                Logger.Info(ex, $"Validation error {ex.Message}");
                throw;
            }
            catch (UnauthorizedException ex)
            {
                Logger.Info(ex, $"Unauthorized error {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, ex.Message);
                throw;
            }
        }

        public async Task PatchCommitment(long providerId, long commitmentId, CommitmentStatus? status)
        {
            try
            {
                await _mediator.SendAsync(new UpdateCommitmentStatusCommand
                {
                    
                    Caller = new Caller
                    {
                        CallerType = CallerType.Provider,
                        Id = providerId
                    },
                    CommitmentId = commitmentId,
                    Status = status
                });
            }
            catch (ValidationException ex)
            {
                Logger.Info(ex, $"Validation error {ex.Message}");
                throw;
            }
            catch (UnauthorizedException ex)
            {
                Logger.Info(ex, $"Unauthorized error {ex.Message}");
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