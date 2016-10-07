using System;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using NLog;
using SFA.DAS.Commitments.Application.Commands.CreateApprenticeship;
using SFA.DAS.Commitments.Application.Commands.CreateCommitment;
using SFA.DAS.Commitments.Application.Commands.UpdateApprenticeship;
using SFA.DAS.Commitments.Application.Commands.UpdateApprenticeshipStatus;
using SFA.DAS.Commitments.Application.Commands.UpdateCommitmentStatus;
using SFA.DAS.Commitments.Application.Exceptions;
using SFA.DAS.Commitments.Application.Queries.GetApprenticeship;
using SFA.DAS.Commitments.Application.Queries.GetCommitment;
using SFA.DAS.Commitments.Application.Queries.GetCommitments;
using SFA.DAS.Commitments.Domain;
using Apprenticeship = SFA.DAS.Commitments.Api.Types.Apprenticeship;
using ApprenticeshipStatus = SFA.DAS.Commitments.Api.Types.ApprenticeshipStatus;
using Commitment = SFA.DAS.Commitments.Api.Types.Commitment;
using CommitmentStatus = SFA.DAS.Commitments.Api.Types.CommitmentStatus;

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

        public async Task<GetCommitmentsResponse> GetCommitments(long id)
        {
            try
            {
                return await _mediator.SendAsync(new GetCommitmentsRequest
                {
                    Caller = new Caller
                    {
                        CallerType = CallerType.Employer,
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

        public async Task<GetCommitmentResponse> GetCommitment(long accountId, long commitmentId)
        {
            try
            {
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

        public async Task<long> CreateCommitment(long accountId, Commitment commitment)
        {
            try
            {
                return await _mediator.SendAsync(new CreateCommitmentCommand
                {
                    Commitment = commitment
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

        public async Task<GetApprenticeshipResponse> GetApprenticeship(long accountId, long commitmentId, long apprenticeshipId)
        {
            try
            {
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

        public async Task PutApprenticeship(long accountId, long commitmentId, long apprenticeshipId, Apprenticeship apprenticeship)
        {
            try
            {
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

        public async Task PatchCommitment(long accountId, long commitmentId, CommitmentStatus? status)
        {
            try
            {
                await _mediator.SendAsync(new UpdateCommitmentStatusCommand
                {
                    AccountId = accountId,
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

        public async Task PatchApprenticeship(long accountId, long commitmentId, long apprenticeshipId, ApprenticeshipStatus? status)
        {
            try
            {
                await _mediator.SendAsync(new UpdateApprenticeshipStatusCommand
                {
                    AccountId = accountId,
                    CommitmentId = commitmentId,
                    ApprenticeshipId = apprenticeshipId,
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

        public async Task<long> CreateApprenticeship(long accountId, long commitmentId, Apprenticeship apprenticeship)
        {
            try
            {
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