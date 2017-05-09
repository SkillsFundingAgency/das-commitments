using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace SFA.DAS.Commitments.Infrastructure.Data
{
    public abstract class BaseRepository
    {
        private readonly string _connectionString;

        protected BaseRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        protected async Task<T> WithConnection<T>(Func<SqlConnection, Task<T>> getData)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync(); // Asynchronously open a connection to the database
                    return await getData(connection);
                    // Asynchronously execute getData, which has been passed in as a Func<IDBConnection, Task<T>>
                }
            }
            catch (TimeoutException ex)
            {
                throw new Exception($"{GetType().FullName}.WithConnection() experienced a timeout", ex);
            }
            catch (SqlException ex)
            {
                if (ex.Number == -2) // SQL Server error number for connection timeout
                    throw new Exception($"{GetType().FullName}.WithConnection() experienced a SQL timeout", ex);

                throw new Exception($"{GetType().FullName}.WithConnection() experienced a SQL exception (error code {ex.Number})", ex);
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
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync(); // Asynchronously open a connection to the database
                    using (var trans = connection.BeginTransaction())
                    {
                        var data = await getData(connection, trans);
                        trans.Commit();
                        return data;
                    }
                    // Asynchronously execute getData, which has been passed in as a Func<IDBConnection, Task<T>>
                }
            }
            catch (TimeoutException ex)
            {
                throw new Exception($"{GetType().FullName}.WithConnection() experienced a SQL timeout", ex);
            }
            catch (SqlException ex)
            {
                throw new Exception(
                    $"{GetType().FullName}.WithConnection() experienced a SQL exception (not a timeout)", ex);
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
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync(); // Asynchronously open a connection to the database
                    using (var trans = connection.BeginTransaction())
                    {
                        await command(connection, trans);
                        trans.Commit();
                    }
                }
            }
            catch (TimeoutException ex)
            {
                throw new Exception($"{GetType().FullName}.WithConnection() experienced a SQL timeout", ex);
            }
            catch (SqlException ex)
            {
                throw new Exception(
                    $"{GetType().FullName}.WithConnection() experienced a SQL exception (not a timeout)", ex);
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"{GetType().FullName}.WithConnection() experienced an exception (not a SQL Exception)", ex);
            }
        }
    }
}