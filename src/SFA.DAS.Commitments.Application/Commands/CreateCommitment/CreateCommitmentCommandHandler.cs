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
                newConnection.SqlConnection = new SqlConnection(connectionString);

                await newConnection.SqlConnection.OpenAsync();

                newConnection.SqlTransaction = newConnection.CreateTransaction(isolationLevel);

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
                return SqlConnection.BeginTransaction(isolationLevel.Value);

            return SqlConnection.BeginTransaction();
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

            if (SqlTransaction != null)
            {
                SqlTransaction.Rollback();
                SqlTransaction = CreateTransaction(_isolationLevel);
            }
        }

        public void Dispose()
        {
            SqlTransaction?.Dispose();
            SqlConnection?.Dispose();
        }

        public SqlConnection SqlConnection { get; private set; }
        public SqlTransaction SqlTransaction { get; private set; }
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

    //todo: make idempotent?
    //todo: integration tests (with fault injection using triggers?)
    //todo: unit tests with fault injection

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
                throw new ValidationException(validationResult.Errors);

            Commitment newCommitment;
            using (var connection = await _unitOfWork.CreateConnection())
            {
                newCommitment = await CreateCommitment(connection, message);

                await Task.WhenAll(
                    CreateMessageIfNeeded(connection, newCommitment.Id, message),
                    CreateHistory(connection, newCommitment, message.Caller.CallerType, message.UserId, message.Commitment.LastUpdatedByEmployerName)
                );
            }

            //todo: ideally publish event within transaction above
            await PublishCohortCreatedEvent(newCommitment);

            return newCommitment.Id;
        }

        private async Task PublishCohortCreatedEvent(Commitment newCommitment)
        {
            await _messagePublisher.PublishAsync(new CohortCreated(newCommitment.EmployerAccountId, newCommitment.ProviderId,
                newCommitment.Id));
        }

        // we could split preparatory work from the actual call to the db and only 'pull the trigger' during the transaction
        // but that would introduce perhaps unnecessary complexity (a premature optimisation)
        // the option is there if required though

        private async Task<Commitment> CreateCommitment(Connection connection, CreateCommitmentCommand message)
        {
            var newCommitment = message.Commitment;
            newCommitment.LastAction = LastAction.None;

            newCommitment.Id = await _commitmentRepository.Create(connection.SqlConnection, connection.SqlTransaction, newCommitment);

            await _commitmentRepository.UpdateCommitmentReference(newCommitment.Id,
                _hashingService.HashValue(newCommitment.Id));
            return newCommitment;
        }

        private async Task CreateHistory(Connection connection, Commitment newCommitment, CallerType callerType, string userId, string userName)
        {
            var historyService = new HistoryService(_historyRepository);
            historyService.TrackInsert(newCommitment, CommitmentChangeType.Created.ToString(), newCommitment.Id, null, callerType, userId, newCommitment.ProviderId, newCommitment.EmployerAccountId, userName);
            await historyService.Save(connection.SqlConnection, connection.SqlTransaction);
        }

        private async Task CreateMessageIfNeeded(Connection connection, long commitmentId, CreateCommitmentCommand command)
        {
            if (string.IsNullOrEmpty(command.Message))
                return;

            var message = new Message
            {
                Author = command.Commitment.LastUpdatedByEmployerName,
                Text = command.Message,
                CreatedBy = command.Caller.CallerType
            };

            await _commitmentRepository.SaveMessage(connection.SqlConnection, connection.SqlTransaction, commitmentId, message);
        }
    }
}
