using System;
using System.Collections.Generic;
using System.Linq;
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
    [TestFixture]
    public class WhenSimulatingRealWorldApprenticeshipLoad
    {
        //todo: better handling of testids
        [Test]
        public async Task SimulateSlowdownScenario()
        {
            const int numberOfGetApprenticeshipTasks = 30, getApprenticeshipCallsPerTask = 6,
                numberOfGetApprenticeshipsTasks = 1, getApprenticeshipsCallsPerTask = 3;

            await SetUpFixture.LogProgress("Starting SimulateSlowdownScenario() Test");
            await SetUpFixture.LogProgress("Generating call parameters");

            var getApprenticeshipCallParamsPerTaskTask = GetGetApprenticeshipCallParamsPerTask(numberOfGetApprenticeshipTasks, getApprenticeshipCallsPerTask);

            var employerIdsPerTaskTask = GetGetApprenticeshipsCallParamsPerTask(numberOfGetApprenticeshipsTasks, getApprenticeshipsCallsPerTask);
            var employerIdsPerTask = await employerIdsPerTaskTask;

            var getApprenticeshipCallParamsPerTask = await getApprenticeshipCallParamsPerTaskTask;
            var callParamsPerTask = getApprenticeshipCallParamsPerTask as IEnumerable<ApprenticeshipCallParams>[] ?? getApprenticeshipCallParamsPerTask.ToArray();

            await SetUpFixture.LogProgress("Spinning up server");

            // pay the cost of test server setup etc. now, so the first result in our timings isn't out
            // todo: do this in setup
            var firstCallParams = callParamsPerTask.First().First();
            await CommitmentsApi.CallGetApprenticeship(firstCallParams.ApprenticeshipId, firstCallParams.EmployerId);

            await SetUpFixture.LogProgress("Starting scenario");

            var tasks = callParamsPerTask.Select(ids => Task.Run(() => RepeatCallGetApprenticeship(ids)))
                .Concat(employerIdsPerTask.Select(ids => Task.Run(() => RepeatCallGetApprenticeships(ids))));

            var callTimesPerTask = await Task.WhenAll(tasks);

            var getApprenticeshipCalls = callTimesPerTask.Take(numberOfGetApprenticeshipTasks).SelectMany(cd => cd);
            var slowestGetApprenticeshipCall = getApprenticeshipCalls.Max(d => d.CallTime);
            var getApprenticeshipsCalls = callTimesPerTask.Skip(numberOfGetApprenticeshipTasks).SelectMany(cd => cd);
            var slowestGetApprenticeshipsCall = getApprenticeshipsCalls.Max(d => d.CallTime);

            var allCalls = getApprenticeshipCalls.Concat(getApprenticeshipsCalls);
            await SetUpFixture.LogProgress("Call Log:");
            // if LogProgress is awaited, the details are written out-of-order
            allCalls.OrderBy(c => c.StartTime).ForEach(c => SetUpFixture.LogProgress(c.ToString()).GetAwaiter().GetResult());

            SetUpFixture.LogProgress($"Slowest GetApprenticeship  {slowestGetApprenticeshipCall}").GetAwaiter().GetResult();
            SetUpFixture.LogProgress($"Slowest GetApprenticeships {slowestGetApprenticeshipsCall}").GetAwaiter().GetResult();

            Assert.LessOrEqual(slowestGetApprenticeshipCall, new TimeSpan(0, 0, 1));
            Assert.LessOrEqual(slowestGetApprenticeshipsCall, new TimeSpan(0, 0, 1));
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
            //Thread.Sleep(2 * 1000);

            var tasks = employerAccountIds.Select(id => CommitmentsApi.CallGetApprenticeships(id));
            return await Task.WhenAll(tasks);
        }

        /// <remarks>
        /// It's better to fetch what we need from the generated data from the db, rather than...
        /// 1) read *all* the db data into memory (a la provider events api in test), as would use an unnecessarily large amount of memory and be slow to read
        /// 2) store it in the database in a similar way to the test ids, as that would require more code, increase db complexity etc. when you'd have to fetch the data from the db anyway
        /// </remarks>
        private async Task<IEnumerable<IEnumerable<ApprenticeshipCallParams>>> GetGetApprenticeshipDistinctCallParamsPerTask(int numberOfTasks, int getApprenticeshipCallsPerTask)
        {
            var alreadyUsedIds = new HashSet<long>(SetUpFixture.TestIds.Ids);

            var totalApprenticeshipIds = numberOfTasks * getApprenticeshipCallsPerTask;
            Assert.GreaterOrEqual(TestDataVolume.MinNumberOfApprenticeships, totalApprenticeshipIds);

            var apprenticeshipIds = new List<long>();
            for (var taskNo = 0; taskNo < totalApprenticeshipIds; ++taskNo)
            {
                var randomApprenticeshipId = (await SetUpFixture.CommitmentsDatabase.GetRandomApprenticeshipIds(1, alreadyUsedIds)).First();
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

        /// <remarks>
        /// It's better to fetch what we need from the generated data from the db, rather than...
        /// 1) read *all* the db data into memory (a la provider events api in test), as would use an unnecessarily large amount of memory and be slow to read
        /// 2) store it in the database in a similar way to the test ids, as that would require more code, increase db complexity etc. when you'd have to fetch the data from the db anyway
        /// </remarks>
        private async Task<IEnumerable<IEnumerable<ApprenticeshipCallParams>>> GetGetApprenticeshipCallParamsPerTask(int numberOfTasks, int getApprenticeshipCallsPerTask)
        {
            var totalApprenticeshipIds = numberOfTasks * getApprenticeshipCallsPerTask;
            Assert.GreaterOrEqual(TestDataVolume.MinNumberOfApprenticeships, totalApprenticeshipIds);

            var apprenticeshipIds = await SetUpFixture.CommitmentsDatabase.GetRandomApprenticeshipIds(totalApprenticeshipIds);

            var employerIdTasks = apprenticeshipIds.Select(id => SetUpFixture.CommitmentsDatabase.GetEmployerId(id));
            var employerIds = await Task.WhenAll(employerIdTasks);

            var callParams = apprenticeshipIds.Zip(employerIds, (apprenticeshipId, employerId) => new ApprenticeshipCallParams
            {
                ApprenticeshipId = apprenticeshipId,
                EmployerId = employerId
            });

            return callParams.Batch(getApprenticeshipCallsPerTask);
        }

        private async Task<IEnumerable<IEnumerable<long>>> GetGetApprenticeshipsCallParamsPerTask(int numberOfTasks, int getApprenticeshipsCallsPerTask)
        {
            // currently have 1:1 ids for cohort:employer in test data, so we can supply the cohort id as the employer id. might have to do better than that, i.e. employer with multiple cohorts, employer with none? perhaps not for our purposes
            var employerIdWithMaxCohortSize = await SetUpFixture.CommitmentsDatabase.GetEmployerId(SetUpFixture.TestIds[TestIds.MaxCohortSize]);
            var employerIds = Enumerable.Repeat(employerIdWithMaxCohortSize, numberOfTasks * getApprenticeshipsCallsPerTask);

            return employerIds.Batch(getApprenticeshipsCallsPerTask);
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

        // when using this pattern...

        // var employerIdTasks = apprenticeshipIds.Select(id => SetUpFixture.CommitmentsDatabase.GetEmployerId(id));
        // var employerIds = await Task.WhenAll(employerIdTasks);

        // it might throw SqlException : The request limit for the database is 60 and has been reached
        // https://blogs.technet.microsoft.com/latam/2015/06/01/how-to-deal-with-the-limits-of-azure-sql-database-maximum-logins/
        // or throw SqlException : Execution Timeout Expired
        // depending on how quickly each db query takes, and how many are kicked off in 1 go
        // if so, we'd want a generic version of this (with callbacks)...

        //{
        //  const int sqlQueryBatchSize = 20;

        //  var apprenticeshipIds = Enumerable.Empty<long>();

        //  int callsLeft = totalApprenticeshipIds;
        //  while (callsLeft > 0)
        //  {
        //      int calls = Math.Min(callsLeft, sqlQueryBatchSize);
        //      callsLeft -= calls;
        //      var apprenticeshipIdsBatchTasks = Enumerable.Range(0, calls).Select(i => SetUpFixture.CommitmentsDatabase.GetRandomApprenticeshipId());
        //      var apprenticeshipIdsBatch = await Task.WhenAll(apprenticeshipIdsBatchTasks);
        //      apprenticeshipIds.Concat(apprenticeshipIdsBatch);
        //  }
        //}
    }
}
