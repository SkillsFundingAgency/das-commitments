using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MoreLinq;
using NUnit.Framework;
using SFA.DAS.Commitments.Api.Controllers;
using SFA.DAS.Commitments.Api.DependencyResolution;
using SFA.DAS.Commitments.Api.IntegrationTests.ApiHost;
using SFA.DAS.Commitments.Api.IntegrationTests.DatabaseSetup;
using SFA.DAS.Commitments.Api.IntegrationTests.Helpers;

namespace SFA.DAS.Commitments.Api.IntegrationTests.Tests
{
    //todo: sut doesn't always pick up httpcontextbase in CurrentNestedContainer
    //todo: all start times are basically at the same time. how to determine when async call is actually made? level of indirection

    [TestFixture]
    public class WhenSimulatingRealWorldApprenticeshipLoad
    {
        [Test]
        public async Task SimulateSlowdownScenario()
        {
            const int numberOfTasks = 8, getApprenticeshipCallsPerTask = 25;

            var getApprenticeshipCallParamsPerTask = await GetGetApprenticeshipCallParamsPerTask(numberOfTasks, getApprenticeshipCallsPerTask);
            var callParamsPerTask = getApprenticeshipCallParamsPerTask as IEnumerable<ApprenticeshipCallParams>[] ?? getApprenticeshipCallParamsPerTask.ToArray();

            // currently have 1:1 ids for cohort:employer in test data, so we can supply the cohort id as the employer id. might have to do better than that, i.e. employer with multiple cohorts, employer with none? perhaps not for our purposes
            var employerIds = new[] { await SetUpFixture.CommitmentsDatabase.GetEmployerId(SetUpFixture.TestIds[TestIds.MaxCohortSize]) };

            // pay the cost of test server setup etc. now, so the first result in our timings isn't out
            var firstCallParams = callParamsPerTask.First().First();
            await CommitmentsApi.CallGetApprenticeship(firstCallParams.ApprenticeshipId, firstCallParams.EmployerId);

            var tasks = callParamsPerTask.Select(ids => Task.Run(() => RepeatCallGetApprenticeship(ids)))
                .Concat(new [] {Task.Run(() => RepeatCallGetApprenticeships(employerIds))});

            var callTimesPerTask = await Task.WhenAll(tasks);

            var slowestGetApprenticeshipCall = callTimesPerTask.Take(numberOfTasks).SelectMany(cd => cd).Max(d => d.CallTime);
            var getApprenticechipsCall = callTimesPerTask.Skip(numberOfTasks).First().Max(d => d.CallTime);

            Assert.LessOrEqual(slowestGetApprenticeshipCall, new TimeSpan(0, 0, 1));
            Assert.LessOrEqual(getApprenticechipsCall, new TimeSpan(0, 0, 1));
        }

        private class ApprenticeshipCallParams
        {
            public long ApprenticeshipId { get; set; }
            public long EmployerId { get; set; }
        }

        private static async Task<CallDetails[]> RepeatCallGetApprenticeship(IEnumerable<ApprenticeshipCallParams> ids)
        {
            var tasks = ids.Select(i => CommitmentsApi.CallGetApprenticeship(i.ApprenticeshipId, i.EmployerId));
            return await Task.WhenAll(tasks);
        }

        public static async Task<CallDetails[]> RepeatCallGetApprenticeships(IEnumerable<long> employerAccountIds)
        {
            //ideally want to use some sort of synchronization, so can kick this off in middle of getapprenticeship calls

            //q&d
            Thread.Sleep(2 * 1000);

            var tasks = employerAccountIds.Select(id => CommitmentsApi.CallGetApprenticeships(id, false));
            return await Task.WhenAll(tasks);
        }

        /// <remarks>
        /// It's better to fetch what we need from the generated data from the db, rather than...
        /// 1) read *all* the db data into memory (a la provider events api in test), as would use an unnecessarily large amount of memory and be slow to read
        /// 2) store it in the database in a similar way to the test ids, as that would require more code, increase db complexity etc. when you'd have to fetch the data from the db anyway
        /// </remarks>
        private async Task<IEnumerable<IEnumerable<ApprenticeshipCallParams>>> GetGetApprenticeshipCallParamsPerTask(int numberOfTasks, int getApprenticeshipCallsPerTask)
        {
            var alreadyUsedIds = new HashSet<long>(SetUpFixture.TestIds.Ids);

            //todo: gracefully report when there is not enough test data to do what we require

            var totalApprenticeshipIds = numberOfTasks * getApprenticeshipCallsPerTask;

            var apprenticeshipIds = new List<long>();
            for (var taskNo = 0; taskNo < totalApprenticeshipIds; ++taskNo)
            {
                var randomApprenticeshipId = TestData.GetRandomApprenticeshipId(alreadyUsedIds);
                apprenticeshipIds.Add(randomApprenticeshipId);

                alreadyUsedIds.Add(randomApprenticeshipId);
            }

            var employerIdTasks = apprenticeshipIds.Select(id => SetUpFixture.CommitmentsDatabase.GetEmployerId(id));

            var employerIds = await Task.WhenAll(employerIdTasks);

            var callParams = apprenticeshipIds.Zip(employerIds, (apprenticeshipId, employerId) => new ApprenticeshipCallParams
            {
                ApprenticeshipId = apprenticeshipId,
                EmployerId = employerId
            });

            return callParams.Batch(getApprenticeshipCallsPerTask);
        }

        [Test]
        [Ignore("Example only")]
        public async Task ExampleTestWhereServiceIsNotSelfHosted()
        {
            long employerAccountId = 8315;
            long apprenticeshipId = 1;

            var container = IoC.Initialize();
            container.Configure(c => c.AddRegistry<TestRegistry>());

            var employerController = container.GetInstance<EmployerController>();
            await employerController.GetApprenticeship(employerAccountId, apprenticeshipId);
        }
    }
}
