using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NServiceBus.Persistence;
using NServiceBus.Settings;
using SFA.DAS.NServiceBus.Features.ClientOutbox.Data;
using SFA.DAS.NServiceBus.Features.ClientOutbox.Models;
using SFA.DAS.NServiceBus.Services;
using SFA.DAS.NServiceBus.SqlServer.Data;
using SFA.DAS.NServiceBus.SqlServer.Features.ClientOutbox.Data;

namespace SFA.DAS.CommitmentsV2.Data
{
    [ExcludeFromCodeCoverage]
    public class ClientOutboxPersisterV2 : IClientOutboxStorageV2
    {
        private readonly IDateTimeService _dateTimeService;
        private readonly Func<DbConnection> _connectionBuilder;

        public ClientOutboxPersisterV2(IDateTimeService dateTimeService, ReadOnlySettings settings)
        {
            _dateTimeService = dateTimeService;
            _connectionBuilder = settings.Get<Func<DbConnection>>("SqlPersistence.ConnectionBuilder");
        }

        public async Task<IClientOutboxTransaction> BeginTransactionAsync()
        {
            var dbConnection = await _connectionBuilder.OpenConnectionAsync().ConfigureAwait(continueOnCapturedContext: false);
            var transaction = await dbConnection.BeginTransactionAsync();
            
            return new SqlClientOutboxTransaction(dbConnection, transaction);
        }

        public async Task<ClientOutboxMessageV2> GetAsync(Guid messageId, SynchronizedStorageSession synchronizedStorageSession)
        {
            var sqlStorageSession = synchronizedStorageSession.GetSqlStorageSession();
            await using var command = sqlStorageSession.Connection.CreateCommand();
          
            command.CommandText = "SELECT EndpointName, Operations FROM dbo.ClientOutboxData WHERE MessageId = @MessageId";
            command.CommandType = CommandType.Text;
            command.Transaction = sqlStorageSession.Transaction;
            command.AddParameter("MessageId", messageId);
           
            await using var reader = await command.ExecuteReaderAsync(CommandBehavior.SingleRow).ConfigureAwait(continueOnCapturedContext: false);
            
            if (await reader.ReadAsync().ConfigureAwait(continueOnCapturedContext: false))
            {
                var @string = reader.GetString(0);
                var transportOperations = JsonConvert.DeserializeObject<List<TransportOperation>>(reader.GetString(1), new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.Auto
                });
                return new ClientOutboxMessageV2(messageId, @string, transportOperations);
            }

            throw new KeyNotFoundException($"Client outbox data not found where MessageId = '{messageId}'");
        }

        public async Task<IEnumerable<IClientOutboxMessageAwaitingDispatch>> GetAwaitingDispatchAsync()
        {
            await using var connection = await _connectionBuilder.OpenConnectionAsync().ConfigureAwait(continueOnCapturedContext: false);
            await using var command = connection.CreateCommand();
         
            command.CommandText = "SELECT MessageId, EndpointName FROM dbo.ClientOutboxData WHERE Dispatched = 0 AND CreatedAt <= @CreatedAt AND PersistenceVersion = '2.0.0' ORDER BY CreatedAt";
            command.CommandType = CommandType.Text;
            command.AddParameter("CreatedAt", _dateTimeService.UtcNow.AddSeconds(-10.0));
            
            await using var reader = await command.ExecuteReaderAsync().ConfigureAwait(continueOnCapturedContext: false);
            var clientOutboxMessages = new List<IClientOutboxMessageAwaitingDispatch>();
            
            while (await reader.ReadAsync().ConfigureAwait(continueOnCapturedContext: false))
            {
                var guid = reader.GetGuid(0);
                var @string = reader.GetString(1);
                var item = new ClientOutboxMessageV2(guid, @string);
                clientOutboxMessages.Add(item);
            }

            return clientOutboxMessages;
        }

        public async Task SetAsDispatchedAsync(Guid messageId)
        {
            await using var connection = await _connectionBuilder.OpenConnectionAsync().ConfigureAwait(continueOnCapturedContext: false);
            await using var command = connection.CreateCommand();
          
            command.CommandText = "UPDATE dbo.ClientOutboxData SET Dispatched = 1, DispatchedAt = @DispatchedAt, Operations = '[]' WHERE MessageId = @MessageId";
            command.CommandType = CommandType.Text;
            command.AddParameter("MessageId", messageId);
            command.AddParameter("DispatchedAt", _dateTimeService.UtcNow);
            
            await command.ExecuteNonQueryAsync().ConfigureAwait(continueOnCapturedContext: false);
        }

        public async Task SetAsDispatchedAsync(Guid messageId, SynchronizedStorageSession synchronizedStorageSession)
        {
            var sqlStorageSession = synchronizedStorageSession.GetSqlStorageSession();
            await using var dbCommand = sqlStorageSession.Connection.CreateCommand();
           
            dbCommand.CommandText = "UPDATE dbo.ClientOutboxData SET Dispatched = 1, DispatchedAt = @DispatchedAt, Operations = '[]' WHERE MessageId = @MessageId";
            dbCommand.CommandType = CommandType.Text;
            dbCommand.Transaction = sqlStorageSession.Transaction;
            dbCommand.AddParameter("MessageId", messageId);
            dbCommand.AddParameter("DispatchedAt", _dateTimeService.UtcNow);
            
            await dbCommand.ExecuteNonQueryAsync();
        }

        public async Task StoreAsync(ClientOutboxMessageV2 clientOutboxMessage, IClientOutboxTransaction clientOutboxTransaction)
        {
            var sqlClientOutboxTransaction = (SqlClientOutboxTransaction)clientOutboxTransaction;
            await using var dbCommand = sqlClientOutboxTransaction.Connection.CreateCommand();
           
            dbCommand.CommandText = "INSERT INTO dbo.ClientOutboxData (MessageId, EndpointName, CreatedAt, PersistenceVersion, Operations) VALUES (@MessageId, @EndpointName, @CreatedAt, '2.0.0', @Operations)";
            dbCommand.CommandType = CommandType.Text;
            dbCommand.Transaction = sqlClientOutboxTransaction.Transaction;
            dbCommand.AddParameter("MessageId", clientOutboxMessage.MessageId);
            dbCommand.AddParameter("EndpointName", clientOutboxMessage.EndpointName);
            dbCommand.AddParameter("CreatedAt", _dateTimeService.UtcNow);
            dbCommand.AddParameter("Operations", JsonConvert.SerializeObject(clientOutboxMessage.TransportOperations, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto
            }));
            
            await dbCommand.ExecuteNonQueryAsync();
        }

        public async Task RemoveEntriesOlderThanAsync(DateTime oldest, CancellationToken cancellationToken)
        {
            await using var connection = await _connectionBuilder.OpenConnectionAsync().ConfigureAwait(continueOnCapturedContext: false);
            
            var flag = false;
           
            while (!cancellationToken.IsCancellationRequested && !flag)
            {
                await using var command = connection.CreateCommand();
                command.CommandText = "DELETE TOP(@BatchSize) FROM dbo.ClientOutboxData WHERE Dispatched = 1 AND DispatchedAt < @DispatchedBefore AND PersistenceVersion = '2.0.0'";
                command.AddParameter("DispatchedBefore", oldest);
                command.AddParameter("BatchSize", 10000);
                flag = await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false) < 10000;
            }
        }
    }
}
