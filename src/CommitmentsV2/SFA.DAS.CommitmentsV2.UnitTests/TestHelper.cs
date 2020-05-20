using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Newtonsoft.Json;
using SFA.DAS.CommitmentsV2.Data;

namespace SFA.DAS.CommitmentsV2.UnitTests
{
    public class TestHelper
    {
        public static T Clone<T>(T source)
        {
            var settings = new JsonSerializerSettings()
            {
                ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor
            };

            var serialized = JsonConvert.SerializeObject(source, settings);
            return JsonConvert.DeserializeObject<T>(serialized, settings);
        }

        public static ProviderCommitmentsDbContext GetInMemoryDatabase() => new ProviderCommitmentsDbContext(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(warnings => warnings.Throw(RelationalEventId.QueryClientEvaluationWarning))
            .EnableSensitiveDataLogging()
            .Options);
    }
}
