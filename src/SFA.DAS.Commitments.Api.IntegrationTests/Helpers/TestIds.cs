
// better name for ns & class?

using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;

namespace SFA.DAS.Commitments.Api.IntegrationTests.Helpers
{
    public class TestIds
    {
//instead of this have property for each??
        public const string MaxCohortSize = "MaxCohortSize";
        //these will be fetched from the db if its already populated
        //public long MaxCohortSize { get; set; }

        private readonly Dictionary<string, long> _ids = new Dictionary<string, long>();

        public TestIds()
        {
        }

        private TestIds(Dictionary<string, long> ids)
        {
            _ids = ids;
        }

        public long this[string key]
        {
            get => _ids[key];
            set => _ids[key] = value;
        }

        private class NamedId
        {
#pragma warning disable 0649
            public string Name;
            public long ObjectId;
#pragma warning restore 0649
        }

        public static async Task<TestIds> Fetch(string databaseConnectionString)
        {
            using (var connection = new SqlConnection(databaseConnectionString))
            {
                await connection.OpenAsync();
                var namedIds = await connection.QueryAsync<NamedId>("select * from dbo.IntegrationTestIds");
                return new TestIds(namedIds.ToDictionary(ni => ni.Name, ni => ni.ObjectId));
            }
        }

        public async Task Store(string databaseConnectionString)
        {
            using (var connection = new SqlConnection(databaseConnectionString))
            {
                await connection.OpenAsync();
                await connection.ExecuteAsync(@"if not exists(select Id from dbo.IntegrationTestIds where Name = @Key)
                                                insert dbo.IntegrationTestIds (Name, ObjectId) values (@Key, @Value)", _ids.ToArray());
            }
        }
    }
}
