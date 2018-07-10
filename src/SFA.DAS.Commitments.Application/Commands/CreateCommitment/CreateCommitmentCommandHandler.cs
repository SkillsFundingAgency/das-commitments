using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Polly;
using Polly.Retry;
using SFA.DAS.Commitments.Application.Services;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Entities.History;
using SFA.DAS.Commitments.Domain.Interfaces;
using SFA.DAS.Commitments.Events;
using Commitment = SFA.DAS.Commitments.Domain.Entities.Commitment;
using LastAction = SFA.DAS.Commitments.Domain.Entities.LastAction;
using SFA.DAS.HashingService;
using SFA.DAS.Messaging.Interfaces;
using SFA.DAS.NLog.Logger;

namespace SFA.DAS.Commitments.Application.Commands.CreateCommitment
{
    public interface IUnitOfWork
    {
        Task<Connection> CreateConnection(IsolationLevel isolationLevel = IsolationLevel.Snapshot);
    }

    ////not an uof, just another repo?
    public class UnitOfWork : IUnitOfWork
    {
        public string CommonConnectionString { get; }

        private readonly ILog _logger;

        // retry per unit, or per repository call?
        // instead of call-back, would be better to create, and disposable with using? Transaction
        public UnitOfWork(string commonConnectionString, ILog logger)
        {
            CommonConnectionString = commonConnectionString;
            _logger = logger;
        }

        public async Task<Connection> CreateConnection(IsolationLevel isolationLevel = IsolationLevel.Snapshot)
        {
            return await Connection.Create(CommonConnectionString, _logger, isolationLevel);
        }
    }

    //rename
    public class Connection : IDisposable
    {
        // version that accepts transaction for distributed transactions?
        // version without isolationlevel, or make nullable

        //enum instead of bool?
        public static async Task<Connection> Create(string connectionString, ILog logger, IsolationLevel? isolationLevel = IsolationLevel.Snapshot)
        {
            var newConnection = new Connection(isolationLevel, logger);

            try
            {
                newConnection._sqlConnection = new SqlConnection(connectionString);

                await newConnection._sqlConnection.OpenAsync();

                newConnection._sqlTransaction = newConnection.CreateTransaction(isolationLevel);

                return newConnection;
            }
            catch (Exception e)
            {
                logger.Error(e, $"Unable to create connection: {e}");
                newConnection.Dispose();
                throw;
            }
        }

        // TransactionScope(... , TransactionScopeAsyncFlowOption.Enabled) instead?
        private SqlTransaction CreateTransaction(IsolationLevel? isolationLevel = IsolationLevel.Snapshot)
        {
            if (isolationLevel.HasValue)
                return _sqlConnection.BeginTransaction(isolationLevel.Value);

            return _sqlConnection.BeginTransaction();
        }

        //public void Commit()
        //{
        //    _sqlTransaction?.Commit();
        //}

        private readonly ILog _logger;
        private readonly Policy _retryPolicy;
        private static readonly HashSet<int> TransientErrorNumbers = new HashSet<int>
        {
            // https://docs.microsoft.com/en-us/azure/sql-database/sql-database-develop-error-messages
            // https://docs.microsoft.com/en-us/azure/sql-database/sql-database-connectivity-issues
            4060, 40197, 40501, 40613, 49918, 49919, 49920, 11001,
            -2, 20, 64, 233, 10053, 10054, 10060, 40143
        };

        // retry is per unit of work, need to rollback on fail
        protected async Task WithRetry(Func<Task> command)
        {
            try
            {
                await _retryPolicy.ExecuteAsync(async () =>
                {
                    await command();
                });
            }
            catch (TimeoutException ex)
            {
                throw new Exception($"{GetType().FullName}.WithConnection() experienced a SQL timeout", ex);
            }
            catch (SqlException ex) when (TransientErrorNumbers.Contains(ex.Number))
            {
                throw new Exception($"{GetType().FullName}.WithConnection() experienced a transient SQL Exception. ErrorNumber {ex.Number}", ex);
            }
            catch (SqlException ex)
            {
                throw new Exception($"{GetType().FullName}.WithConnection() experienced a non-transient SQL exception (error code {ex.Number})", ex);
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"{GetType().FullName}.WithConnection() experienced an exception (not a SQL Exception)", ex);
            }
        }

        private RetryPolicy GetRetryPolicy()
        {
            return Policy
                .Handle<SqlException>(ex => TransientErrorNumbers.Contains(ex.Number))
                .Or<TimeoutException>()
                .WaitAndRetryAsync(3, retryAttempt =>
                        TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    (exception, timespan, retryCount, context) => OnRetry(exception, retryCount));
        }

        private void OnRetry(Exception exception, int retryCount)
        {
            //context.CorrelationId ??

            _logger.Warn($"Retrying...attempt {retryCount}. Exception: {exception}");

            //ordering / interaction

            _cancellationTokenSource.Cancel();
            _cancellationTokenSource = new CancellationTokenSource();

            if (_sqlTransaction != null)
            {
                _sqlTransaction.Rollback();
                _sqlTransaction = CreateTransaction(_isolationLevel);
            }
        }

        public void Dispose()
        {
            _sqlTransaction?.Dispose();
            _sqlConnection?.Dispose();
        }

        private SqlConnection _sqlConnection;
        private SqlTransaction _sqlTransaction;
        //private CancellationToken _cancellationToken;
        private CancellationTokenSource _cancellationTokenSource;
        private readonly IsolationLevel? _isolationLevel;

        //private Connection(string connectionString, bool createTransaction,
        //    IsolationLevel isolationLevel = IsolationLevel.Snapshot)
        //{
        //}

        private Connection(IsolationLevel? isolationLevel, ILog logger)
        {
            _isolationLevel = isolationLevel;
            _logger = logger;
            _cancellationTokenSource = new CancellationTokenSource();
            //_cancellationToken = _cancellationTokenSource.Token;
            _retryPolicy = GetRetryPolicy();
        }
    }

    public sealed class CreateCommitmentCommandHandler : IAsyncRequestHandler<CreateCommitmentCommand, long>
    {
        private readonly AbstractValidator<CreateCommitmentCommand> _validator;
        private readonly ICommitmentRepository _commitmentRepository;
        private readonly IHashingService _hashingService;
        private readonly ICommitmentsLogger _logger;
        private readonly IHistoryRepository _historyRepository;
        private readonly IMessagePublisher _messagePublisher;
        private readonly IUnitOfWork _unitOfWork;

        public CreateCommitmentCommandHandler(ICommitmentRepository commitmentRepository, IHashingService hashingService, AbstractValidator<CreateCommitmentCommand> validator, ICommitmentsLogger logger, IHistoryRepository historyRepository, IMessagePublisher messagePublisher,
            IUnitOfWork unitOfWork)
        {
            _commitmentRepository = commitmentRepository;
            _hashingService = hashingService;
            _validator = validator;
            _logger = logger;
            _historyRepository = historyRepository;
            _messagePublisher = messagePublisher;
            _unitOfWork = unitOfWork;
        }

        public async Task<long> Handle(CreateCommitmentCommand message)
        {
            _logger.Info($"Employer: {message.Commitment.EmployerAccountId} has called CreateCommitmentCommand", accountId: message.Commitment.EmployerAccountId);

            var validationResult = _validator.Validate(message);

            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            using (_unitOfWork.CreateConnection())
            {
                var newCommitment = await CreateCommitment(message);

                await Task.WhenAll(
                    CreateMessageIfNeeded(newCommitment.Id, message),
                    CreateHistory(newCommitment, message.Caller.CallerType, message.UserId,
                        message.Commitment.LastUpdatedByEmployerName), PublishCohortCreatedEvent(newCommitment)
                );

                return newCommitment.Id;
            }
        }

        private async Task PublishCohortCreatedEvent(Commitment newCommitment)
        {
            await _messagePublisher.PublishAsync(new CohortCreated(newCommitment.EmployerAccountId, newCommitment.ProviderId,
                newCommitment.Id));
        }

        private async Task<Commitment> CreateCommitment(CreateCommitmentCommand message)
        {
            var newCommitment = message.Commitment;
            newCommitment.LastAction = LastAction.None;

            newCommitment.Id = await _commitmentRepository.Create(newCommitment);

            await _commitmentRepository.UpdateCommitmentReference(newCommitment.Id,
                _hashingService.HashValue(newCommitment.Id));
            return newCommitment;
        }

        private async Task CreateHistory(Commitment newCommitment, CallerType callerType, string userId, string userName)
        {
            var historyService = new HistoryService(_historyRepository);
            historyService.TrackInsert(newCommitment, CommitmentChangeType.Created.ToString(), newCommitment.Id, null, callerType, userId, newCommitment.ProviderId, newCommitment.EmployerAccountId, userName);
            await historyService.Save();
        }

        private async Task CreateMessageIfNeeded(long commitmentId, CreateCommitmentCommand command)
        {
            if (string.IsNullOrEmpty(command.Message))
                return;

            var message = new Message
            {
                Author = command.Commitment.LastUpdatedByEmployerName,
                Text = command.Message,
                CreatedBy = command.Caller.CallerType
            };

            await _commitmentRepository.SaveMessage(commitmentId, message);
        }
    }
}
