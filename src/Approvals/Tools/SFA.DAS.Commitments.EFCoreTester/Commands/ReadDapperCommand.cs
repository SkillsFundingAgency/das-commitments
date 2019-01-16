using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using SFA.DAS.Commitments.EFCoreTester.Data;
using SFA.DAS.Commitments.EFCoreTester.Data.Models;
using SFA.DAS.Commitments.EFCoreTester.Interfaces;

namespace SFA.DAS.Commitments.EFCoreTester.Commands
{
    public class ReadDapperCommand : ICommand
    {
        private readonly ITimer _timer;

        public ReadDapperCommand(ITimer timer)
        {
            _timer = timer;
        }

        public Task DoAsync(CancellationToken cancellationToken)
        {
            using (var db = CreateSqlConnection())
            {
                _timer.Time("Read drafts", () => ReadApprenticeships<DraftApprenticeship>(db, 0));
                _timer.Time("Read confirmed", () => ReadApprenticeships<ConfirmedApprenticeship>(db, 1));
            }

            return Task.CompletedTask;
        }

        private int ReadApprenticeships<T>(SqlConnection sqlConnection, short paymentStatus) where T: Apprenticeship
        {
            var parameters = new DynamicParameters();
            parameters.Add("@paymentStatus", paymentStatus);

            var apprenticeships = sqlConnection.Query<T>(
                                    "select * from dbo.Apprenticeship where PaymentStatus = @paymentStatus", parameters, commandType: CommandType.Text)
                                    .ToList();

            return apprenticeships.Count;
        }

        private SqlConnection CreateSqlConnection()
        {
            return _timer.Time("Create Sql Connection", () => new SqlConnection(Constants.ConnectionString));
        }
    }
}
