using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using SFA.DAS.Commitments.Infrastructure.Configuration;

namespace SFA.DAS.Commitments.Infrastructure.Data
{
    public abstract class BaseRepository
    {
        private readonly string _connectionString;

        protected BaseRepository(CommitmentConfiguration configuration)
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            _connectionString = configuration.DatabaseConnectionString;
        }

        protected async Task<T> WithConnection<T>(Func<IDbConnection, Task<T>> getData)
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