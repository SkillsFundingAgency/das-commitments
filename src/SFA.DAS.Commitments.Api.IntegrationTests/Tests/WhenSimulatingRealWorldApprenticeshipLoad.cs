using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using MoreLinq;
using Newtonsoft.Json;
using NUnit.Framework;
using SFA.DAS.Commitments.Api.Controllers;
using SFA.DAS.Commitments.Api.DependencyResolution;
using SFA.DAS.Commitments.Api.IntegrationTests.ApiHost;
using SFA.DAS.Commitments.Api.IntegrationTests.DatabaseSetup;
using SFA.DAS.Commitments.Api.IntegrationTests.DatabaseSetup.Entities;
using SFA.DAS.Commitments.Api.IntegrationTests.Helpers;
using SFA.DAS.Commitments.Api.Orchestrators;
using SFA.DAS.Commitments.Api.Types.Apprenticeship;

namespace SFA.DAS.Commitments.Api.IntegrationTests.Tests
{
    //todo: infrastructure for integration tests (test helpers/inmemory self hosting) - remove need to authorization for requests from localhost? or in-memory?
    //todo: infrastructure generating mass data(autofixture?/sqlbulkcopy?/schema changes)
    //todo: write scenarios(parallel execution)
    //todo: use INTTEST as env? so could have a seperate database to contain all the test data
    // /\ think we need this so we can blatt the data when necessary
    //todo: sut doesn't always pick up httpcontextbase in CurrentNestedContainer
    //todo: check testids always get written when they should
    //todo: not reproducing slowdown. try suffling datalockstatus, run against at azure sql db (not db in ram on 56g 8 core machine!), what else? check when calls are actually made
    //todo: all start times are basically at the same time. how to determine when async call is actually made? level of indirection

    [TestFixture]
    public class WhenSimulatingRealWorldApprenticeshipLoad
    {
        private static async Task<CallDetails[]> RepeatCallGetApprenticeship(IEnumerable<ApprenticeshipCallParams> ids)
        {
            var tasks = ids.Select(i => CommitmentsApi.CallGetApprenticeship(i.ApprenticeshipId, i.EmployerId));
            return await Task.WhenAll(tasks);
        }

        //private static async Task<CallDetails[]> RepeatCallGetApprenticeships(ICollection<ApprenticeshipCallParams> ids)
        //{
        //    var tasks = ids.Select(i => CommitmentsApi.CallGetApprenticeship(i.ApprenticeshipId, i.EmployerId));
        //    return await Task.WhenAll(tasks);
        //}

        public static async Task<CallDetails[]> RepeatCallGetApprenticeships(IEnumerable<long> employerAccountIds)
        {
            //ideally want to use some sort of synchronization, so can kick this off in middle of getapprenticeship calls

            //q&d
            Thread.Sleep(2*1000);

            var tasks = employerAccountIds.Select(CommitmentsApi.CallGetApprenticeships);
            return await Task.WhenAll(tasks);
        }

        private class ApprenticeshipCallParams
        {
            public long ApprenticeshipId { get; set; }
            public long EmployerId { get; set; }
        }

        [Test]
        public async Task SimulateSlowdownScenario()
        {
            const int numberOfTasks = 8;
            const int getApprenticeshipCallsPerTask = 25;

            //todo: rename now callparams not apprenticeshipids
            var getApprenticeshipIdsPerTask = await GetGetApprenticeshipCallParamsPerTask(numberOfTasks, getApprenticeshipCallsPerTask);
            var apprenticeshipIdsPerTask = getApprenticeshipIdsPerTask as IEnumerable<ApprenticeshipCallParams>[] ?? getApprenticeshipIdsPerTask.ToArray();

            //currently have 1:1 cohort:employer, so we can supply the cohort id as the employer id. might have to do better than that, i.e. employer with multiple cohorts, employer with none? perhaps not for our purposes
            var employerIds = new[] { await SetUpFixture.CommitmentsDatabase.GetEmployerId(SetUpFixture.TestIds[TestIds.MaxCohortSize]) };
            //tasks = tasks.Concat(new[] { CommitmentsApi.CallGetApprenticeships(employerIds) });

            // pay the cost of test server setup etc. now, so the first result in our timings isn't out
            await CommitmentsApi.CallGetApprenticeship(apprenticeshipIdsPerTask.First().First().ApprenticeshipId,
                apprenticeshipIdsPerTask.First().First().EmployerId);

            var tasks = apprenticeshipIdsPerTask.Select(ids => Task.Run(() => RepeatCallGetApprenticeship(ids)))
                .Concat(new [] {Task.Run(() => RepeatCallGetApprenticeships(employerIds))});

            var callTimesPerTask = await Task.WhenAll(tasks);

            var slowestGetApprenticeshipCall = callTimesPerTask.Take(numberOfTasks).SelectMany(cd => cd).Max(d => d.CallTime);
            var getApprenticechipsCall = callTimesPerTask.Skip(numberOfTasks).First().Max(d => d.CallTime);

            Assert.LessOrEqual(slowestGetApprenticeshipCall, new TimeSpan(0, 0, 1));
            Assert.LessOrEqual(getApprenticechipsCall, new TimeSpan(0, 0, 1));
        }

        private async Task<IEnumerable<IEnumerable<ApprenticeshipCallParams>>> GetGetApprenticeshipCallParamsPerTask(int numberOfTasks, int getApprenticeshipCallsPerTask)
        {
            var alreadyUsedIds = new HashSet<long>(SetUpFixture.TestIds.Ids);

            //todo: gracefully report when there is not enough test data to do what we require

            var totalApprenticeshipIds = numberOfTasks * getApprenticeshipCallsPerTask;

            var apprenticeshipIds = new List<long>();
            for (var taskNo = 0; taskNo < totalApprenticeshipIds; ++taskNo)
            {
                var randomApprenticeshipId = GetRandomApprenticeshipId(alreadyUsedIds);
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
        public async Task SimulateSlowdownScenarioOld()
        {
            //best way to handle concurrency? async only? tpl parallel? threads? other?
            //todo: async calls below are all on 1 thread. need to e.g. start x threads calling getapprentice, and then while they are going trigger getapprenticeships on other thread
            var tasks = new List<Task<CallDetails>>();

            const int preGetApprenticeshipsGetApprenticeshipCalls = 50;
            const int postGetApprenticeshipsGetApprenticeshipCalls = 50;

            // better to fetch what we need from the generated data from the db, rather than
            // 1) read all the db data into memory (a la provider events api in test), as would use an unnecessarily large amount of memory)
            // 2) store it in the database in a similar way to the test ids, as that would require more code, increase db complexity etc. when you'd have to fetch the data from the db anyway
            var getApprentishipIdsTasks = Enumerable.Repeat(0, preGetApprenticeshipsGetApprenticeshipCalls + postGetApprenticeshipsGetApprenticeshipCalls)
                .Select(async x =>
                {
                    var ApprenticeshipId = GetRandomApprenticeshipId();
                    return new
                    {
                        ApprenticeshipId,
                        EmpoyerId = await SetUpFixture.CommitmentsDatabase.GetEmployerId(ApprenticeshipId)
                    };
                });

            var getApprenticeshipIds = await Task.WhenAll(getApprentishipIdsTasks);

            // pay the cost of test server setup etc. now, so the first result in our timings isn't out
            await CommitmentsApi.CallGetApprenticeship(getApprenticeshipIds.First().ApprenticeshipId, getApprenticeshipIds.First().EmpoyerId);

            //better to just for?
            //for (var preCall = 0; preCall < preGetApprenticeshipsGetApprenticeshipCalls; ++preCall)
            //{
            //}

            //for (var preCall = 0; preCall < preGetApprenticeshipsGetApprenticeshipCalls; ++preCall)
            //{
            //    var apprenticeshipId = GetRandomApprenticeshipId();
            //    tasks.Add(CommitmentsApi.CallGetApprenticeship(apprenticeshipId, apprenticeshipId));
            //}

            tasks.AddRange(getApprenticeshipIds.Take(preGetApprenticeshipsGetApprenticeshipCalls)
                .Select(ids => CommitmentsApi.CallGetApprenticeship(ids.ApprenticeshipId, ids.EmpoyerId)));

            //currently have 1:1 cohort:employer, might have to do better than that, i.e. employer with multiple cohorts, employer with none? perhaps not for our purposes
            tasks.Add(CommitmentsApi.CallGetApprenticeships(SetUpFixture.TestIds[TestIds.MaxCohortSize]));

            //for (var postCall = 0; postCall < postGetApprenticeshipsGetApprenticeshipCalls; ++postCall)
            //{
            //    var apprenticeshipId = GetRandomApprenticeshipId();
            //    tasks.Add(CommitmentsApi.CallGetApprenticeship(apprenticeshipId, apprenticeshipId));
            //}

            tasks.AddRange(getApprenticeshipIds.Skip(preGetApprenticeshipsGetApprenticeshipCalls)
                .Select(ids => CommitmentsApi.CallGetApprenticeship(ids.ApprenticeshipId, ids.EmpoyerId)));

            var callTimes = await Task.WhenAll(tasks);
            var slowestGetApprenticeshipCall = callTimes.Take(preGetApprenticeshipsGetApprenticeshipCalls)
                .Concat(callTimes.Skip(preGetApprenticeshipsGetApprenticeshipCalls+1)).Max(d => d.CallTime);
            var getApprenticechipsCall = callTimes.Skip(preGetApprenticeshipsGetApprenticeshipCalls).First().CallTime;

            Assert.LessOrEqual(slowestGetApprenticeshipCall, new TimeSpan(0,0,1));
            Assert.LessOrEqual(getApprenticechipsCall, new TimeSpan(0,0,1));
        }

        private static readonly Random Random = new Random();
        public long GetRandomApprenticeshipId(HashSet<long> exclude = null)
        {
            if (exclude == null)
                return Random.Next(1, TestDataVolume.MinNumberOfApprenticeships + 1);

            long apprenticeshipId;
            while (exclude.Contains(apprenticeshipId = Random.Next(1, TestDataVolume.MinNumberOfApprenticeships + 1)))
            {
            }

            return apprenticeshipId;
        }

        [Test]
        public async Task NotSelfHosted()
        {
            //todo: the test will have to create these of course mf
            long employerAccountId = 8315;
            long apprenticeshipId = 1;

            var container = IoC.Initialize();
            container.Configure(c => c.AddRegistry<TestRegistry>());

            var employerController = container.GetInstance<EmployerController>();
            var apprenticeship = await employerController.GetApprenticeship(employerAccountId, apprenticeshipId);
        }
    }
}
