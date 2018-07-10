using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Polly;
using Polly.Retry;
using SFA.DAS.NLog.Logger;

namespace SFA.DAS.Commitments.Application.Commands.CreateCommitment
{
    //temporary
    public abstract class NuBaseRepository
    {
        private readonly string _connectionString;
        private readonly ILog _logger;
        private readonly Policy _retryPolicy;
        private static readonly HashSet<int> TransientErrorNumbers = new HashSet<int>
            {
                // https://docs.microsoft.com/en-us/azure/sql-database/sql-database-develop-error-messages
                // https://docs.microsoft.com/en-us/azure/sql-database/sql-database-connectivity-issues
                4060, 40197, 40501, 40613, 49918, 49919, 49920, 11001,
                -2, 20, 64, 233, 10053, 10054, 10060, 40143
            };

        protected NuBaseRepository(string connectionString, ILog logger)
        {
            _connectionString = connectionString;
            _logger = logger;

            _retryPolicy = GetRetryPolicy();
        }

        protected async Task<T> WithConnection<T>(Func<SqlConnection, Task<T>> getData)
        {
            try
            {
                return await _retryPolicy.ExecuteAsync(async () =>
                {
                    using (var connection = new SqlConnection(_connectionString))
                    {
                        await connection.OpenAsync();
                        return await getData(connection);
                    }
                });
            }
            catch (TimeoutException ex)
            {
                throw new Exception($"{GetType().FullName}.WithConnection() experienced a timeout", ex);
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

        protected async Task<T> WithTransaction<T>(Func<IDbConnection, IDbTransaction, Task<T>> getData)
        {
            try
            {
                return await _retryPolicy.ExecuteAsync(async () =>
                {
                    using (var connection = new SqlConnection(_connectionString))
                    {
                        await connection.OpenAsync();
                        using (var trans = connection.BeginTransaction())
                        {
                            var data = await getData(connection, trans);
                            trans.Commit();
                            return data;
                        }
                    }
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

        protected async Task WithTransaction(Func<IDbConnection, IDbTransaction, Task> command)
        {
            try
            {
                await _retryPolicy.ExecuteAsync(async () =>
                {
                    using (var connection = new SqlConnection(_connectionString))
                    {
                        await connection.OpenAsync();
                        using (var trans = connection.BeginTransaction())
                        {
                            await command(connection, trans);
                            trans.Commit();
                        }
                    }
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

        // bother passing conn & tran? rename to ExecuteWithRetry?
        //protected async Task WithExistingTransaction(IDbConnection connection, IDbTransaction transaction, Func<IDbConnection, IDbTransaction, Task> command)
        //{
        //    try
        //    {
        //        await _retryPolicy.ExecuteAsync(async () =>
        //        {
        //            await command(connection, transaction);
        //        });
        //    }
        //    catch (TimeoutException ex)
        //    {
        //        throw new Exception($"{GetType().FullName}.WithConnection() experienced a SQL timeout", ex);
        //    }
        //    catch (SqlException ex) when (TransientErrorNumbers.Contains(ex.Number))
        //    {
        //        throw new Exception($"{GetType().FullName}.WithConnection() experienced a transient SQL Exception. ErrorNumber {ex.Number}", ex);
        //    }
        //    catch (SqlException ex)
        //    {
        //        throw new Exception($"{GetType().FullName}.WithConnection() experienced a non-transient SQL exception (error code {ex.Number})", ex);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception(
        //            $"{GetType().FullName}.WithConnection() experienced an exception (not a SQL Exception)", ex);
        //    }
        //}

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
                    (exception, timespan, retryCount, context) =>
                    {
                        _logger.Warn($"SqlException ({exception.Message}). Retrying...attempt {retryCount})");
                    }
                );
        }
    }
}
