using AutoFixture;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Application.Queries.GetChangeOfProviderChain;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.UnitOfWork.Context;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Queries.GetChangeOfProviderChain
{
    [TestFixture]
    [Parallelizable]
    public class GetChangeOfProviderChainQueryHandlerTests
    {
        [TestCaseSource(typeof(CreateChangeOfProviderChainDataCases))]
        public async Task Handle_WhenRequested_ThenItShouldReturnTheChangeOfProviderChain(Dictionary<long, long[]> expectedOutputMap, CreateChangeOfProviderChainDataCases.Input[] inputs, Dictionary<long, CreateChangeOfProviderChainDataCases.ExpectedOutput> expectedOutputs)
        {
            var fixture = new GetChangeOfProviderQueryHandlerTestFixtures();
            foreach(var input in inputs)
            {
                fixture.SetupCohort(input);
            }

            foreach (var expectedOutput in expectedOutputMap)
            {
                var filteredExpectedOutputs = expectedOutputs
                    .Where(p => expectedOutput.Value.Contains(p.Key))
                    .Select(p => p.Value);

                var results = await fixture.Handle(expectedOutput.Key);
                Assert.IsTrue(TestHelpers.CompareHelper.AreEqualIgnoringTypes(results.ChangeOfProviderChain, filteredExpectedOutputs));
            }
        }
    }

    public class CreateChangeOfProviderChainDataCases : IEnumerable
    {
        private Provider Provider101 = new Provider { ProviderId = 101, ProviderName = "Provider101" };
        private Provider Provider102 = new Provider { ProviderId = 102, ProviderName = "Provider102" };
        private Provider Provider103 = new Provider { ProviderId = 103, ProviderName = "Provider103" };
        private Provider Provider201 = new Provider { ProviderId = 201, ProviderName = "Provider201" };
        private Provider Provider202 = new Provider { ProviderId = 202, ProviderName = "Provider202" };

        private DateTime Now = DateTime.Now;

        public IEnumerator GetEnumerator()
        {
            // emplyoer 1 (provider 1)
            yield return new object[] {
                new Dictionary<long, long[]>
                {
                    { 10001, new long[] { 1 } }  // employer 1 can see provider 1
                },
                new Input[]
                {
                    new Input { ApprenticeshipId = 10001, EmployerAccountId = 200, StartDate = Now.AddDays(5), EndDate = Now.AddDays(25), StopDate = null, CreatedOn = Now, ContinuationOfId = null, CurrentProvider = Provider101, NewProvider = null, OriginatingParty = Party.Employer, ChangeOfPartyRequestType = null},
                },
                new Dictionary<long, ExpectedOutput>
                {
                    { 1, new ExpectedOutput { ApprenticeshipId = 10001, EmployerAccountId = 200, ProviderName = Provider101.ProviderName, StartDate = Now.AddDays(5), EndDate = Now.AddDays(25), StopDate = null, CreatedOn = null, ContinuationOfId = null, NewApprenticeshipId = null } }
                }
            };

            // employer 1 (provider 1) => employer 1 (provider 2) 
            yield return new object[] {
                new Dictionary<long, long[]>
                {
                    { 10001, new long[] { 1, 2 } },  // employer 1 can see provider 1 => 2
                    { 10002, new long[] { 1, 2 } }   // employer 1 can see provider 1 <= 2
                },
                new Input[]
                {
                    new Input { ApprenticeshipId = 10001, EmployerAccountId = 200, StartDate = Now.AddDays(5), EndDate = Now.AddDays(25), StopDate = Now.AddDays(5), CreatedOn = Now, ContinuationOfId = null, CurrentProvider = Provider101, NewProvider = Provider102, OriginatingParty = Party.Employer, ChangeOfPartyRequestType = ChangeOfPartyRequestType.ChangeProvider },
                    new Input { ApprenticeshipId = 10002, EmployerAccountId = 200, StartDate = Now.AddDays(6), EndDate = Now.AddDays(25), StopDate = null, CreatedOn = Now.AddDays(1), ContinuationOfId = 10001, CurrentProvider = Provider102, NewProvider = null, OriginatingParty = Party.Employer, ChangeOfPartyRequestType = null},
                },
                new Dictionary<long, ExpectedOutput>
                {
                    { 1,  new ExpectedOutput { ApprenticeshipId = 10002, EmployerAccountId = 200, ProviderName = Provider102.ProviderName, StartDate = Now.AddDays(6), EndDate = Now.AddDays(25), StopDate = null, CreatedOn = null, ContinuationOfId = 10001, NewApprenticeshipId = null } },
                    { 2,  new ExpectedOutput { ApprenticeshipId = 10001, EmployerAccountId = 200, ProviderName = Provider101.ProviderName, StartDate = Now.AddDays(5), EndDate = Now.AddDays(25), StopDate = Now.AddDays(5), CreatedOn = Now, ContinuationOfId = null, NewApprenticeshipId = 10002 } }
                }
            };

            // employer 1 (provider 1) => employer 1 (provider 2) => employer 1 (provider 3)
            yield return new object[] {
                new Dictionary<long, long[]>
                {
                    { 10001, new long[] { 1, 2, 3 } },  // employer 1 can see provider 1 => 2 => 3
                    { 10002, new long[] { 1, 2, 3 } },  // employer 1 can see provider 1 <= 2 => 3
                    { 10003, new long[] { 1, 2, 3 } }   // employer 1 can see provider 1 <= 2 <= 3
                },
                new Input[]
                {
                    new Input { ApprenticeshipId = 10001, EmployerAccountId = 200, StartDate = Now.AddDays(5), EndDate = Now.AddDays(25), StopDate = Now.AddDays(5), CreatedOn = Now, ContinuationOfId = null, CurrentProvider = Provider101, NewProvider = Provider102, OriginatingParty = Party.Employer, ChangeOfPartyRequestType = ChangeOfPartyRequestType.ChangeProvider },
                    new Input { ApprenticeshipId = 10002, EmployerAccountId = 200, StartDate = Now.AddDays(6), EndDate = Now.AddDays(25), StopDate = Now.AddDays(6), CreatedOn = Now.AddDays(1), ContinuationOfId = 10001, CurrentProvider = Provider102, NewProvider = Provider103, OriginatingParty = Party.Employer, ChangeOfPartyRequestType = ChangeOfPartyRequestType.ChangeProvider },
                    new Input { ApprenticeshipId = 10003, EmployerAccountId = 200, StartDate = Now.AddDays(7), EndDate = Now.AddDays(25), StopDate = null, CreatedOn = Now.AddDays(2), ContinuationOfId = 10002, CurrentProvider = Provider103, NewProvider = null, OriginatingParty = Party.Employer, ChangeOfPartyRequestType = null},
                },
                new Dictionary<long, ExpectedOutput>
                {
                    { 1, new ExpectedOutput { ApprenticeshipId = 10003, EmployerAccountId = 200, ProviderName = Provider103.ProviderName, StartDate = Now.AddDays(7), EndDate = Now.AddDays(25), StopDate = null, CreatedOn = null, ContinuationOfId = 10002, NewApprenticeshipId = null } },
                    { 2, new ExpectedOutput { ApprenticeshipId = 10002, EmployerAccountId = 200, ProviderName = Provider102.ProviderName, StartDate = Now.AddDays(6), EndDate = Now.AddDays(25), StopDate = Now.AddDays(6), CreatedOn = Now.AddDays(1), ContinuationOfId = 10001, NewApprenticeshipId = 10003 } },
                    { 3, new ExpectedOutput { ApprenticeshipId = 10001, EmployerAccountId = 200, ProviderName = Provider101.ProviderName, StartDate = Now.AddDays(5), EndDate = Now.AddDays(25), StopDate = Now.AddDays(5), CreatedOn = Now, ContinuationOfId = null, NewApprenticeshipId = 10002 } }
                }
            };

            // employer 1 (provider 1) => employer 1 (provider 2) => employer 2 (provider 2) => employer 2 (provider 3) => employer 1 (provider 3)
            yield return new object[] {
                new Dictionary<long, long[]>
                {
                    { 10001, new long[] { 1, 4, 5 } },  // employer 1 can see provider 1 => 2 => 3
                    { 10002, new long[] { 1, 4, 5 } },  // employer 1 can see provider 1 <= 2 => 3
                    { 10003, new long[] { 2, 3 } },     // employer 2 can see provider 2 => 3
                    { 10004, new long[] { 2, 3 } },     // employer 2 can see provider 2 <= 3
                    { 10005, new long[] { 1, 4, 5 } }   // employer 1 can see provider 1 <= 2 <= 3
                },
                new Input[]
                {
                    new Input { ApprenticeshipId = 10001, EmployerAccountId = 200, StartDate = Now.AddDays(5), EndDate = Now.AddDays(25), StopDate = Now.AddDays(5), CreatedOn = Now, ContinuationOfId = null, CurrentProvider = Provider101, NewProvider = Provider102, OriginatingParty = Party.Employer, ChangeOfPartyRequestType = ChangeOfPartyRequestType.ChangeProvider},
                    new Input { ApprenticeshipId = 10002, EmployerAccountId = 200, StartDate = Now.AddDays(6), EndDate = Now.AddDays(25), StopDate = Now.AddDays(6), CreatedOn = Now.AddDays(1), ContinuationOfId = 10001, CurrentProvider = Provider102, NewProvider = Provider102, OriginatingParty = Party.Provider, ChangeOfPartyRequestType = ChangeOfPartyRequestType.ChangeEmployer},
                    new Input { ApprenticeshipId = 10003, EmployerAccountId = 300, StartDate = Now.AddDays(7), EndDate = Now.AddDays(25), StopDate = Now.AddDays(7), CreatedOn = Now.AddDays(2), ContinuationOfId = 10002, CurrentProvider = Provider102, NewProvider = Provider103, OriginatingParty = Party.Employer, ChangeOfPartyRequestType = ChangeOfPartyRequestType.ChangeProvider},
                    new Input { ApprenticeshipId = 10004, EmployerAccountId = 300, StartDate = Now.AddDays(8), EndDate = Now.AddDays(25), StopDate = Now.AddDays(8), CreatedOn = Now.AddDays(3), ContinuationOfId = 10003, CurrentProvider = Provider103, NewProvider = Provider103, OriginatingParty = Party.Provider, ChangeOfPartyRequestType = ChangeOfPartyRequestType.ChangeEmployer},
                    new Input { ApprenticeshipId = 10005, EmployerAccountId = 200, StartDate = Now.AddDays(9), EndDate = Now.AddDays(25), StopDate = null, CreatedOn = Now.AddDays(4), ContinuationOfId = 10004, CurrentProvider = Provider103, NewProvider = null, OriginatingParty = Party.Employer, ChangeOfPartyRequestType = null},
                },
                new Dictionary<long, ExpectedOutput>
                {
                    { 1, new ExpectedOutput { ApprenticeshipId = 10005, EmployerAccountId = 200, ProviderName = Provider103.ProviderName, StartDate = Now.AddDays(9), EndDate = Now.AddDays(25), StopDate = null, CreatedOn = null, ContinuationOfId = 10004, NewApprenticeshipId = null } },
                    { 2, new ExpectedOutput { ApprenticeshipId = 10004, EmployerAccountId = 300, ProviderName = Provider103.ProviderName, StartDate = Now.AddDays(8), EndDate = Now.AddDays(25), StopDate = Now.AddDays(8), CreatedOn = Now.AddDays(3), ContinuationOfId = 10003, NewApprenticeshipId = 10005 } },
                    { 3, new ExpectedOutput { ApprenticeshipId = 10003, EmployerAccountId = 300, ProviderName = Provider102.ProviderName, StartDate = Now.AddDays(7), EndDate = Now.AddDays(25), StopDate = Now.AddDays(7), CreatedOn = Now.AddDays(2), ContinuationOfId = 10002, NewApprenticeshipId = 10004 } },
                    { 4, new ExpectedOutput { ApprenticeshipId = 10002, EmployerAccountId = 200, ProviderName = Provider102.ProviderName, StartDate = Now.AddDays(6), EndDate = Now.AddDays(25), StopDate = Now.AddDays(6), CreatedOn = Now.AddDays(1), ContinuationOfId = 10001, NewApprenticeshipId = 10003 } },
                    { 5, new ExpectedOutput { ApprenticeshipId = 10001, EmployerAccountId = 200, ProviderName = Provider101.ProviderName, StartDate = Now.AddDays(5), EndDate = Now.AddDays(25), StopDate = Now.AddDays(5), CreatedOn = Now, ContinuationOfId = null, NewApprenticeshipId = 10002 } }
                }
            };

            // employer 1 (provider 1) => employer 1 (provider 2) => employer 2 (provider 2) => employer 2 (provider 3) => employer 2 (provider 4) =>
            // employer 1 (provider 4) => employer 1 (provider 5)
            yield return new object[] {
                new Dictionary<long, long[]>
                {
                    { 10001, new long[] { 1, 2, 6, 7 } },  // employer 1 can see provider 1 => 2 => 4 => 5
                    { 10002, new long[] { 1, 2, 6, 7 } },  // employer 1 can see provider 1 <= 2 => 4 => 5
                    { 10003, new long[] { 3, 4, 5 } },     // employer 2 can see provider 2 => 3 => 4
                    { 10004, new long[] { 3, 4, 5 } },     // employer 2 can see provider 2 <= 3 => 4
                    { 10005, new long[] { 3, 4, 5 } },     // employer 2 can see provider 2 <= 3 <= 4
                    { 10006, new long[] { 1, 2, 6, 7 } },  // employer 1 can see provider 1 <= 2 <= 4 => 5
                    { 10007, new long[] { 1, 2, 6, 7 } }   // employer 1 can see provider 1 <= 2 <= 4 <= 5
                },
                new Input[]
                {
                    new Input { ApprenticeshipId = 10001, EmployerAccountId = 200, StartDate = Now.AddDays(5), EndDate = Now.AddDays(25), StopDate = Now.AddDays(5), CreatedOn = Now, ContinuationOfId = null, CurrentProvider = Provider101, NewProvider = Provider102, OriginatingParty = Party.Employer, ChangeOfPartyRequestType = ChangeOfPartyRequestType.ChangeProvider},
                    new Input { ApprenticeshipId = 10002, EmployerAccountId = 200, StartDate = Now.AddDays(6), EndDate = Now.AddDays(25), StopDate = Now.AddDays(6), CreatedOn = Now.AddDays(1), ContinuationOfId = 10001, CurrentProvider = Provider102, NewProvider = Provider102, OriginatingParty = Party.Provider, ChangeOfPartyRequestType = ChangeOfPartyRequestType.ChangeEmployer},
                    new Input { ApprenticeshipId = 10003, EmployerAccountId = 300, StartDate = Now.AddDays(7), EndDate = Now.AddDays(25), StopDate = Now.AddDays(7), CreatedOn = Now.AddDays(2), ContinuationOfId = 10002, CurrentProvider = Provider102, NewProvider = Provider103, OriginatingParty = Party.Employer, ChangeOfPartyRequestType = ChangeOfPartyRequestType.ChangeProvider},
                    new Input { ApprenticeshipId = 10004, EmployerAccountId = 300, StartDate = Now.AddDays(8), EndDate = Now.AddDays(25), StopDate = Now.AddDays(8), CreatedOn = Now.AddDays(3), ContinuationOfId = 10003, CurrentProvider = Provider103, NewProvider = Provider201, OriginatingParty = Party.Employer, ChangeOfPartyRequestType = ChangeOfPartyRequestType.ChangeProvider},
                    new Input { ApprenticeshipId = 10005, EmployerAccountId = 300, StartDate = Now.AddDays(9), EndDate = Now.AddDays(25), StopDate = Now.AddDays(9), CreatedOn = Now.AddDays(4), ContinuationOfId = 10004, CurrentProvider = Provider201, NewProvider = Provider103, OriginatingParty = Party.Provider, ChangeOfPartyRequestType = ChangeOfPartyRequestType.ChangeEmployer},
                    new Input { ApprenticeshipId = 10006, EmployerAccountId = 200, StartDate = Now.AddDays(10), EndDate = Now.AddDays(25), StopDate = Now.AddDays(10), CreatedOn = Now.AddDays(5), ContinuationOfId = 10005, CurrentProvider = Provider201, NewProvider = Provider202, OriginatingParty = Party.Employer, ChangeOfPartyRequestType = ChangeOfPartyRequestType.ChangeProvider},
                    new Input { ApprenticeshipId = 10007, EmployerAccountId = 200, StartDate = Now.AddDays(11), EndDate = Now.AddDays(25), StopDate = null, CreatedOn = Now.AddDays(6), ContinuationOfId = 10006, CurrentProvider = Provider202, NewProvider = null, OriginatingParty = Party.Employer, ChangeOfPartyRequestType = null},
                },
                new Dictionary<long, ExpectedOutput>
                {
                    { 1, new ExpectedOutput { ApprenticeshipId = 10007, EmployerAccountId = 200, ProviderName = Provider202.ProviderName, StartDate = Now.AddDays(11), EndDate = Now.AddDays(25), StopDate = null, CreatedOn = null, ContinuationOfId = 10006, NewApprenticeshipId = null } },
                    { 2, new ExpectedOutput { ApprenticeshipId = 10006, EmployerAccountId = 200, ProviderName = Provider201.ProviderName, StartDate = Now.AddDays(10), EndDate = Now.AddDays(25), StopDate = Now.AddDays(10), CreatedOn = Now.AddDays(5), ContinuationOfId = 10005, NewApprenticeshipId = 10007 } },
                    { 3, new ExpectedOutput { ApprenticeshipId = 10005, EmployerAccountId = 300, ProviderName = Provider201.ProviderName, StartDate = Now.AddDays(9), EndDate = Now.AddDays(25), StopDate = Now.AddDays(9), CreatedOn = Now.AddDays(4), ContinuationOfId = 10004, NewApprenticeshipId = 10006 } },
                    { 4, new ExpectedOutput { ApprenticeshipId = 10004, EmployerAccountId = 300, ProviderName = Provider103.ProviderName, StartDate = Now.AddDays(8), EndDate = Now.AddDays(25), StopDate = Now.AddDays(8), CreatedOn = Now.AddDays(3), ContinuationOfId = 10003, NewApprenticeshipId = 10005 } },
                    { 5, new ExpectedOutput { ApprenticeshipId = 10003, EmployerAccountId = 300, ProviderName = Provider102.ProviderName, StartDate = Now.AddDays(7), EndDate = Now.AddDays(25), StopDate = Now.AddDays(7), CreatedOn = Now.AddDays(2), ContinuationOfId = 10002, NewApprenticeshipId = 10004 } },
                    { 6, new ExpectedOutput { ApprenticeshipId = 10002, EmployerAccountId = 200, ProviderName = Provider102.ProviderName, StartDate = Now.AddDays(6), EndDate = Now.AddDays(25), StopDate = Now.AddDays(6), CreatedOn = Now.AddDays(1), ContinuationOfId = 10001, NewApprenticeshipId = 10003 } },
                    { 7, new ExpectedOutput { ApprenticeshipId = 10001, EmployerAccountId = 200, ProviderName = Provider101.ProviderName, StartDate = Now.AddDays(5), EndDate = Now.AddDays(25), StopDate = Now.AddDays(5), CreatedOn = Now, ContinuationOfId = null, NewApprenticeshipId = 10002 } }
                }
            };
        }

        public class Provider
        {
            public long ProviderId { get; set; }
            public string ProviderName { get; set; }
        }

        public class Input
        {
            public long ApprenticeshipId { get; set; }
            public long EmployerAccountId { get; set; }
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
            public DateTime? StopDate { get; set; }
            public DateTime CreatedOn { get; set; }
            public long? ContinuationOfId { get; set; }
            public Provider CurrentProvider { get; set; }
            public Provider NewProvider { get; set; }
            public Party OriginatingParty { get; set; }
            public ChangeOfPartyRequestType? ChangeOfPartyRequestType { get; set; }
        }

        public class ExpectedOutput
        {
            public long ApprenticeshipId { get; set; }
            public long EmployerAccountId { get; set; }
            public string ProviderName { get; set; }
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
            public DateTime? StopDate { get; set; }
            public DateTime? CreatedOn { get; set; }
            public long? ContinuationOfId { get; set; }
            public long? NewApprenticeshipId { get; set; }
        }
    }

    public class GetChangeOfProviderQueryHandlerTestFixtures
    {
        public ProviderCommitmentsDbContext Db { get; set; }
        public GetChangeOfProviderChainQueryHandler Handler { get; set; }
        
        public GetChangeOfProviderQueryHandlerTestFixtures()
        {
            Db = new ProviderCommitmentsDbContext(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
            Handler = new GetChangeOfProviderChainQueryHandler(
                new Lazy<ProviderCommitmentsDbContext>(() => Db));
        }

        public Task<GetChangeOfProviderChainQueryResult> Handle(long apprenticeshipId)
        {
            var query = new GetChangeOfProviderChainQuery(apprenticeshipId);
            return Handler.Handle(query, CancellationToken.None);
        }

        public void SetupCohort(CreateChangeOfProviderChainDataCases.Input input)
        {
            // This line is required.
            // ReSharper disable once ObjectCreationAsStatement
            new UnitOfWorkContext();

            var autoFixture = new Fixture();

            var commitment = new Cohort(
                input.CurrentProvider.ProviderId,
                input.EmployerAccountId,
                autoFixture.Create<long>(),
                input.OriginatingParty,
                new UserInfo());

            Db.Cohorts.Add(commitment);
            Db.SaveChanges();

            var apprenticeship = new Apprenticeship
            {
                Id = input.ApprenticeshipId,
                CommitmentId = commitment.Id,
                StartDate = input.StartDate,
                EndDate = input.EndDate,
                StopDate = input.StopDate,
                CreatedOn = input.CreatedOn,
                ContinuationOfId = input.ContinuationOfId
            };
                
            Db.Apprenticeships.Add(apprenticeship);
            Db.SaveChanges();

            if (input.ChangeOfPartyRequestType.HasValue)
            {
                var changeOfPartyRequest = new ChangeOfPartyRequest(
                        apprenticeship,
                        input.ChangeOfPartyRequestType.Value,
                        input.OriginatingParty,
                        input.NewProvider.ProviderId,
                        1000,
                        input.StartDate,
                        input.EndDate,
                        new UserInfo(),
                        input.CreatedOn
                    );

                Db.ChangeOfPartyRequests.Add(changeOfPartyRequest);
                Db.SaveChanges();
            }

            if (input.ContinuationOfId.HasValue)
            {
                var continuationOfApprenticeship =
                    Db.ChangeOfPartyRequests.FirstOrDefault(
                        changeOfPartyRequest => changeOfPartyRequest.ApprenticeshipId == input.ContinuationOfId);

                continuationOfApprenticeship.SetNewApprenticeship(apprenticeship, new UserInfo(), input.OriginatingParty);
                Db.SaveChanges();
            }

            if (input.CurrentProvider != null)
            {
                if (!Db.Providers.Any(p => p.UkPrn == input.CurrentProvider.ProviderId))
                {
                    var provider = new Provider()
                    {
                        UkPrn = input.CurrentProvider.ProviderId,
                        Name = input.CurrentProvider.ProviderName
                    };

                    Db.Providers.Add(provider);
                    Db.SaveChanges();
                }
            }

            if (input.NewProvider != null)
            {
                if (!Db.Providers.Any(p => p.UkPrn == input.NewProvider.ProviderId))
                {
                    var provider = new Provider()
                    {
                        UkPrn = input.NewProvider.ProviderId,
                        Name = input.NewProvider.ProviderName
                    };

                    Db.Providers.Add(provider);
                    Db.SaveChanges();
                }
            }
        }
    }
}