using SFA.DAS.CommitmentsV2.Application.Queries.GetChangeOfEmployerChain;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.UnitOfWork.Context;
using System.Collections;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Queries.GetChangeOfEmployerChain
{
    [TestFixture]
    [Parallelizable]
    public class GetChangeOfEmployerChainQueryHandlerTests
    {
        [TestCaseSource(typeof(CreateChangeOfEmployerChainDataCases))]
        public async Task Handle_WhenRequested_ThenItShouldReturnTheChangeOfEmployerChain(Dictionary<long, long[]> expectedOutputMap, CreateChangeOfEmployerChainDataCases.Input[] inputs, Dictionary<long, CreateChangeOfEmployerChainDataCases.ExpectedOutput> expectedOutputs)
        {
            var fixture = new GetChangeOfEmployerQueryHandlerTestFixtures();
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
                Assert.That(TestHelpers.CompareHelper.AreEqualIgnoringTypes(results.ChangeOfEmployerChain, filteredExpectedOutputs), Is.True);
            }
        }
    }

    public class CreateChangeOfEmployerChainDataCases : IEnumerable
    {
        private Employer Employer101 = new Employer { EmployerId = 101, EmployerName = "Employer101" };
        private Employer Employer102 = new Employer { EmployerId = 102, EmployerName = "Employer102" };
        private Employer Employer103 = new Employer { EmployerId = 103, EmployerName = "Employer103" };
        private Employer Employer201 = new Employer { EmployerId = 201, EmployerName = "Employer201" };
        private Employer Employer202 = new Employer { EmployerId = 202, EmployerName = "Employer202" };
        private Employer EmployerDeleted = new Employer { EmployerId = 301, EmployerName = "EmployerDeleted", IsDeleted = true };

        private DateTime Now = DateTime.Now;

        public IEnumerator GetEnumerator()
        {
            // Provider 1 (Employer 1)
            yield return new object[] {
                new Dictionary<long, long[]>
                {
                    { 10001, new long[] { 1 } }  // Provider 1 can see Employer 1
                },
                new Input[]
                {
                    new Input { ApprenticeshipId = 10001, Ukprn = 200, StartDate = Now.AddDays(5), EndDate = Now.AddDays(25), StopDate = null, CreatedOn = Now, ContinuationOfId = null, CurrentEmployer = Employer101, NewEmployer = null, OriginatingParty = Party.Provider, ChangeOfPartyRequestType = null},
                },
                new Dictionary<long, ExpectedOutput>
                {
                    { 1, new ExpectedOutput { ApprenticeshipId = 10001, Ukprn = 200, EmployerName = Employer101.EmployerName, StartDate = Now.AddDays(5), EndDate = Now.AddDays(25), StopDate = null, CreatedOn = null, ContinuationOfId = null, NewApprenticeshipId = null } }
                }
            };

            // Provider 1 (Employer 1) => Provider 1 (Employer 2) 
            yield return new object[] {
                new Dictionary<long, long[]>
                {
                    { 10001, new long[] { 1, 2 } },  // Provider 1 can see Employer 1 => 2
                    { 10002, new long[] { 1, 2 } }   // Provider 1 can see Employer 1 <= 2
                },
                new Input[]
                {
                    new Input { ApprenticeshipId = 10001, Ukprn = 200, ActualStartDate = Now.AddDays(5), EndDate = Now.AddDays(25), StopDate = Now.AddDays(5), CreatedOn = Now, ContinuationOfId = null, CurrentEmployer = Employer101, NewEmployer = Employer102, OriginatingParty = Party.Provider, ChangeOfPartyRequestType = ChangeOfPartyRequestType.ChangeEmployer },
                    new Input { ApprenticeshipId = 10002, Ukprn = 200, StartDate = Now.AddDays(6), EndDate = Now.AddDays(25), StopDate = null, CreatedOn = Now.AddDays(1), ContinuationOfId = 10001, CurrentEmployer = Employer102, NewEmployer = null, OriginatingParty = Party.Provider, ChangeOfPartyRequestType = null},
                },
                new Dictionary<long, ExpectedOutput>
                {
                    { 1,  new ExpectedOutput { ApprenticeshipId = 10002, Ukprn = 200, EmployerName = Employer102.EmployerName, StartDate = Now.AddDays(6), EndDate = Now.AddDays(25), StopDate = null, CreatedOn = null, ContinuationOfId = 10001, NewApprenticeshipId = null } },
                    { 2,  new ExpectedOutput { ApprenticeshipId = 10001, Ukprn = 200, EmployerName = Employer101.EmployerName, StartDate = Now.AddDays(5), EndDate = Now.AddDays(25), StopDate = Now.AddDays(5), CreatedOn = Now, ContinuationOfId = null, NewApprenticeshipId = 10002 } }
                }
            };

            // Provider 1 (Employer 1) => Provider 1 (Employer 2) => Provider 1 (Employer 3)
            yield return new object[] {
                new Dictionary<long, long[]>
                {
                    { 10001, new long[] { 1, 2, 3 } },  // Provider 1 can see Employer 1 => 2 => 3
                    { 10002, new long[] { 1, 2, 3 } },  // Provider 1 can see Employer 1 <= 2 => 3
                    { 10003, new long[] { 1, 2, 3 } }   // Provider 1 can see Employer 1 <= 2 <= 3
                },
                new Input[]
                {
                    new Input { ApprenticeshipId = 10001, Ukprn = 200, StartDate = Now.AddDays(5), EndDate = Now.AddDays(25), StopDate = Now.AddDays(5), CreatedOn = Now, ContinuationOfId = null, CurrentEmployer = Employer101, NewEmployer = Employer102, OriginatingParty = Party.Provider, ChangeOfPartyRequestType = ChangeOfPartyRequestType.ChangeEmployer },
                    new Input { ApprenticeshipId = 10002, Ukprn = 200, ActualStartDate = Now.AddDays(6), EndDate = Now.AddDays(25), StopDate = Now.AddDays(6), CreatedOn = Now.AddDays(1), ContinuationOfId = 10001, CurrentEmployer = Employer102, NewEmployer = Employer103, OriginatingParty = Party.Provider, ChangeOfPartyRequestType = ChangeOfPartyRequestType.ChangeEmployer },
                    new Input { ApprenticeshipId = 10003, Ukprn = 200, StartDate = Now.AddDays(7), EndDate = Now.AddDays(25), StopDate = null, CreatedOn = Now.AddDays(2), ContinuationOfId = 10002, CurrentEmployer = Employer103, NewEmployer = null, OriginatingParty = Party.Provider, ChangeOfPartyRequestType = null},
                },
                new Dictionary<long, ExpectedOutput>
                {
                    { 1, new ExpectedOutput { ApprenticeshipId = 10003, Ukprn = 200, EmployerName = Employer103.EmployerName, StartDate = Now.AddDays(7), EndDate = Now.AddDays(25), StopDate = null, CreatedOn = null, ContinuationOfId = 10002, NewApprenticeshipId = null } },
                    { 2, new ExpectedOutput { ApprenticeshipId = 10002, Ukprn = 200, EmployerName = Employer102.EmployerName, StartDate = Now.AddDays(6), EndDate = Now.AddDays(25), StopDate = Now.AddDays(6), CreatedOn = Now.AddDays(1), ContinuationOfId = 10001, NewApprenticeshipId = 10003 } },
                    { 3, new ExpectedOutput { ApprenticeshipId = 10001, Ukprn = 200, EmployerName = Employer101.EmployerName, StartDate = Now.AddDays(5), EndDate = Now.AddDays(25), StopDate = Now.AddDays(5), CreatedOn = Now, ContinuationOfId = null, NewApprenticeshipId = 10002 } }
                }
            };

            // Provider 1 (Employer 1) => skip Provider 1 (Employer Deleted) => Provider 1 (Employer 3) - [CON-3774]
            yield return new object[] {
                new Dictionary<long, long[]>
                {
                    { 10001, new long[] { 1, 2, 3 } },  // Provider 1 can see Employer 1 => 2 => 3
                    { 10002, new long[] { 1, 2, 3 } },  // Provider 1 can see Employer 1 <= 2 => 3
                    { 10003, new long[] { 1, 2, 3 } }   // Provider 1 can see Employer 1 <= 2 <= 3
                },
                new Input[]
                {
                    new Input { ApprenticeshipId = 10001, Ukprn = 200, StartDate = Now.AddDays(5), EndDate = Now.AddDays(25), StopDate = Now.AddDays(5), CreatedOn = Now, ContinuationOfId = null, CurrentEmployer = Employer101, NewEmployer = EmployerDeleted, OriginatingParty = Party.Provider, ChangeOfPartyRequestType = ChangeOfPartyRequestType.ChangeEmployer },
                    new Input { ApprenticeshipId = 10002, Ukprn = 200, ActualStartDate = Now.AddDays(6), EndDate = Now.AddDays(25), StopDate = Now.AddDays(6), CreatedOn = Now.AddDays(1), ContinuationOfId = 10001, CurrentEmployer = EmployerDeleted, NewEmployer = Employer103, OriginatingParty = Party.Provider, ChangeOfPartyRequestType = ChangeOfPartyRequestType.ChangeEmployer },
                    new Input { ApprenticeshipId = 10003, Ukprn = 200, StartDate = Now.AddDays(7), EndDate = Now.AddDays(25), StopDate = null, CreatedOn = Now.AddDays(2), ContinuationOfId = 10002, CurrentEmployer = Employer103, NewEmployer = null, OriginatingParty = Party.Provider, ChangeOfPartyRequestType = null},
                },
                new Dictionary<long, ExpectedOutput>
                {
                    { 1, new ExpectedOutput { ApprenticeshipId = 10003, Ukprn = 200, EmployerName = Employer103.EmployerName, StartDate = Now.AddDays(7), EndDate = Now.AddDays(25), StopDate = null, CreatedOn = null, ContinuationOfId = 10002, NewApprenticeshipId = null } },
                    // Do not expect the apprenticeship for the deleted employer
                    { 3, new ExpectedOutput { ApprenticeshipId = 10001, Ukprn = 200, EmployerName = Employer101.EmployerName, StartDate = Now.AddDays(5), EndDate = Now.AddDays(25), StopDate = Now.AddDays(5), CreatedOn = Now, ContinuationOfId = null, NewApprenticeshipId = 10002 } }
                }
            };

            // Provider 1 (Employer 1) => Provider 1 (Employer 2) => Provider 2 (Employer 2) => Provider 2 (Employer 3) => Provider 1 (Employer 3)
            yield return new object[] {
                new Dictionary<long, long[]>
                {
                    { 10001, new long[] { 1, 4, 5 } },  // Provider 1 can see Employer 1 => 2 => 3
                    { 10002, new long[] { 1, 4, 5 } },  // Provider 1 can see Employer 1 <= 2 => 3
                    { 10003, new long[] { 2, 3 } },     // Provider 2 can see Employer 2 => 3
                    { 10004, new long[] { 2, 3 } },     // Provider 2 can see Employer 2 <= 3
                    { 10005, new long[] { 1, 4, 5 } }   // Provider 1 can see Employer 1 <= 2 <= 3
                },
                new Input[]
                {
                    new Input { ApprenticeshipId = 10001, Ukprn = 200, StartDate = Now.AddDays(5), EndDate = Now.AddDays(25), StopDate = Now.AddDays(5), CreatedOn = Now, ContinuationOfId = null, CurrentEmployer = Employer101, NewEmployer = Employer102, OriginatingParty = Party.Provider, ChangeOfPartyRequestType = ChangeOfPartyRequestType.ChangeEmployer},
                    new Input { ApprenticeshipId = 10002, Ukprn = 200, ActualStartDate = Now.AddDays(6), EndDate = Now.AddDays(25), StopDate = Now.AddDays(6), CreatedOn = Now.AddDays(1), ContinuationOfId = 10001, CurrentEmployer = Employer102, NewEmployer = Employer102, OriginatingParty = Party.Employer, ChangeOfPartyRequestType = ChangeOfPartyRequestType.ChangeProvider},
                    new Input { ApprenticeshipId = 10003, Ukprn = 300, StartDate = Now.AddDays(7), EndDate = Now.AddDays(25), StopDate = Now.AddDays(7), CreatedOn = Now.AddDays(2), ContinuationOfId = 10002, CurrentEmployer = Employer102, NewEmployer = Employer103, OriginatingParty = Party.Provider, ChangeOfPartyRequestType = ChangeOfPartyRequestType.ChangeEmployer},
                    new Input { ApprenticeshipId = 10004, Ukprn = 300, ActualStartDate = Now.AddDays(8), EndDate = Now.AddDays(25), StopDate = Now.AddDays(8), CreatedOn = Now.AddDays(3), ContinuationOfId = 10003, CurrentEmployer = Employer103, NewEmployer = Employer103, OriginatingParty = Party.Employer, ChangeOfPartyRequestType = ChangeOfPartyRequestType.ChangeProvider},
                    new Input { ApprenticeshipId = 10005, Ukprn = 200, StartDate = Now.AddDays(9), EndDate = Now.AddDays(25), StopDate = null, CreatedOn = Now.AddDays(4), ContinuationOfId = 10004, CurrentEmployer = Employer103, NewEmployer = null, OriginatingParty = Party.Provider, ChangeOfPartyRequestType = null},
                },
                new Dictionary<long, ExpectedOutput>
                {
                    { 1, new ExpectedOutput { ApprenticeshipId = 10005, Ukprn = 200, EmployerName = Employer103.EmployerName, StartDate = Now.AddDays(9), EndDate = Now.AddDays(25), StopDate = null, CreatedOn = null, ContinuationOfId = 10004, NewApprenticeshipId = null } },
                    { 2, new ExpectedOutput { ApprenticeshipId = 10004, Ukprn = 300, EmployerName = Employer103.EmployerName, StartDate = Now.AddDays(8), EndDate = Now.AddDays(25), StopDate = Now.AddDays(8), CreatedOn = Now.AddDays(3), ContinuationOfId = 10003, NewApprenticeshipId = 10005 } },
                    { 3, new ExpectedOutput { ApprenticeshipId = 10003, Ukprn = 300, EmployerName = Employer102.EmployerName, StartDate = Now.AddDays(7), EndDate = Now.AddDays(25), StopDate = Now.AddDays(7), CreatedOn = Now.AddDays(2), ContinuationOfId = 10002, NewApprenticeshipId = 10004 } },
                    { 4, new ExpectedOutput { ApprenticeshipId = 10002, Ukprn = 200, EmployerName = Employer102.EmployerName, StartDate = Now.AddDays(6), EndDate = Now.AddDays(25), StopDate = Now.AddDays(6), CreatedOn = Now.AddDays(1), ContinuationOfId = 10001, NewApprenticeshipId = 10003 } },
                    { 5, new ExpectedOutput { ApprenticeshipId = 10001, Ukprn = 200, EmployerName = Employer101.EmployerName, StartDate = Now.AddDays(5), EndDate = Now.AddDays(25), StopDate = Now.AddDays(5), CreatedOn = Now, ContinuationOfId = null, NewApprenticeshipId = 10002 } }
                }
            };

            // Provider 1 (Employer 1) => Provider 1 (Employer 2) => Provider 2 (Employer 2) => Provider 2 (Employer 3) => Provider 2 (Employer 4) =>
            // Provider 1 (Employer 4) => Provider 1 (Employer 5)
            yield return new object[] {
                new Dictionary<long, long[]>
                {
                    { 10001, new long[] { 1, 2, 6, 7 } },  // Provider 1 can see Employer 1 => 2 => 4 => 5
                    { 10002, new long[] { 1, 2, 6, 7 } },  // Provider 1 can see Employer 1 <= 2 => 4 => 5
                    { 10003, new long[] { 3, 4, 5 } },     // Provider 2 can see Employer 2 => 3 => 4
                    { 10004, new long[] { 3, 4, 5 } },     // Provider 2 can see Employer 2 <= 3 => 4
                    { 10005, new long[] { 3, 4, 5 } },     // Provider 2 can see Employer 2 <= 3 <= 4
                    { 10006, new long[] { 1, 2, 6, 7 } },  // Provider 1 can see Employer 1 <= 2 <= 4 => 5
                    { 10007, new long[] { 1, 2, 6, 7 } }   // Provider 1 can see Employer 1 <= 2 <= 4 <= 5
                },
                new Input[]
                {
                    new Input { ApprenticeshipId = 10001, Ukprn = 200, StartDate = Now.AddDays(5), EndDate = Now.AddDays(25), StopDate = Now.AddDays(5), CreatedOn = Now, ContinuationOfId = null, CurrentEmployer = Employer101, NewEmployer = Employer102, OriginatingParty = Party.Provider, ChangeOfPartyRequestType = ChangeOfPartyRequestType.ChangeEmployer},
                    new Input { ApprenticeshipId = 10002, Ukprn = 200, ActualStartDate = Now.AddDays(6), EndDate = Now.AddDays(25), StopDate = Now.AddDays(6), CreatedOn = Now.AddDays(1), ContinuationOfId = 10001, CurrentEmployer = Employer102, NewEmployer = Employer102, OriginatingParty = Party.Employer, ChangeOfPartyRequestType = ChangeOfPartyRequestType.ChangeProvider},
                    new Input { ApprenticeshipId = 10003, Ukprn = 300, StartDate = Now.AddDays(7), EndDate = Now.AddDays(25), StopDate = Now.AddDays(7), CreatedOn = Now.AddDays(2), ContinuationOfId = 10002, CurrentEmployer = Employer102, NewEmployer = Employer103, OriginatingParty = Party.Provider, ChangeOfPartyRequestType = ChangeOfPartyRequestType.ChangeEmployer},
                    new Input { ApprenticeshipId = 10004, Ukprn = 300, ActualStartDate = Now.AddDays(8), EndDate = Now.AddDays(25), StopDate = Now.AddDays(8), CreatedOn = Now.AddDays(3), ContinuationOfId = 10003, CurrentEmployer = Employer103, NewEmployer = Employer201, OriginatingParty = Party.Provider, ChangeOfPartyRequestType = ChangeOfPartyRequestType.ChangeEmployer},
                    new Input { ApprenticeshipId = 10005, Ukprn = 300, StartDate = Now.AddDays(9), EndDate = Now.AddDays(25), StopDate = Now.AddDays(9), CreatedOn = Now.AddDays(4), ContinuationOfId = 10004, CurrentEmployer = Employer201, NewEmployer = Employer103, OriginatingParty = Party.Employer, ChangeOfPartyRequestType = ChangeOfPartyRequestType.ChangeProvider},
                    new Input { ApprenticeshipId = 10006, Ukprn = 200, ActualStartDate = Now.AddDays(10), EndDate = Now.AddDays(25), StopDate = Now.AddDays(10), CreatedOn = Now.AddDays(5), ContinuationOfId = 10005, CurrentEmployer = Employer201, NewEmployer = Employer202, OriginatingParty = Party.Provider, ChangeOfPartyRequestType = ChangeOfPartyRequestType.ChangeEmployer},
                    new Input { ApprenticeshipId = 10007, Ukprn = 200, StartDate = Now.AddDays(11), EndDate = Now.AddDays(25), StopDate = null, CreatedOn = Now.AddDays(6), ContinuationOfId = 10006, CurrentEmployer = Employer202, NewEmployer = null, OriginatingParty = Party.Provider, ChangeOfPartyRequestType = null},
                },
                new Dictionary<long, ExpectedOutput>
                {
                    { 1, new ExpectedOutput { ApprenticeshipId = 10007, Ukprn = 200, EmployerName = Employer202.EmployerName, StartDate = Now.AddDays(11), EndDate = Now.AddDays(25), StopDate = null, CreatedOn = null, ContinuationOfId = 10006, NewApprenticeshipId = null } },
                    { 2, new ExpectedOutput { ApprenticeshipId = 10006, Ukprn = 200, EmployerName = Employer201.EmployerName, StartDate = Now.AddDays(10), EndDate = Now.AddDays(25), StopDate = Now.AddDays(10), CreatedOn = Now.AddDays(5), ContinuationOfId = 10005, NewApprenticeshipId = 10007 } },
                    { 3, new ExpectedOutput { ApprenticeshipId = 10005, Ukprn = 300, EmployerName = Employer201.EmployerName, StartDate = Now.AddDays(9), EndDate = Now.AddDays(25), StopDate = Now.AddDays(9), CreatedOn = Now.AddDays(4), ContinuationOfId = 10004, NewApprenticeshipId = 10006 } },
                    { 4, new ExpectedOutput { ApprenticeshipId = 10004, Ukprn = 300, EmployerName = Employer103.EmployerName, StartDate = Now.AddDays(8), EndDate = Now.AddDays(25), StopDate = Now.AddDays(8), CreatedOn = Now.AddDays(3), ContinuationOfId = 10003, NewApprenticeshipId = 10005 } },
                    { 5, new ExpectedOutput { ApprenticeshipId = 10003, Ukprn = 300, EmployerName = Employer102.EmployerName, StartDate = Now.AddDays(7), EndDate = Now.AddDays(25), StopDate = Now.AddDays(7), CreatedOn = Now.AddDays(2), ContinuationOfId = 10002, NewApprenticeshipId = 10004 } },
                    { 6, new ExpectedOutput { ApprenticeshipId = 10002, Ukprn = 200, EmployerName = Employer102.EmployerName, StartDate = Now.AddDays(6), EndDate = Now.AddDays(25), StopDate = Now.AddDays(6), CreatedOn = Now.AddDays(1), ContinuationOfId = 10001, NewApprenticeshipId = 10003 } },
                    { 7, new ExpectedOutput { ApprenticeshipId = 10001, Ukprn = 200, EmployerName = Employer101.EmployerName, StartDate = Now.AddDays(5), EndDate = Now.AddDays(25), StopDate = Now.AddDays(5), CreatedOn = Now, ContinuationOfId = null, NewApprenticeshipId = 10002 } }
                }
            };
        }

        public class Employer
        {
            public long EmployerId { get; set; }
            public string EmployerName { get; set; }
            public bool IsDeleted { get; internal set; }
        }

        public class Input
        {
            public long ApprenticeshipId { get; set; }
            public long Ukprn { get; set; }
            public DateTime? StartDate { get; set; }
            public DateTime? ActualStartDate { get; set; }
            public DateTime EndDate { get; set; }
            public DateTime? StopDate { get; set; }
            public DateTime? EmploymentEndDate { get; set; }
            public DateTime CreatedOn { get; set; }
            public long? ContinuationOfId { get; set; }
            public Employer CurrentEmployer { get; set; }
            public Employer NewEmployer { get; set; }
            public Party OriginatingParty { get; set; }
            public ChangeOfPartyRequestType? ChangeOfPartyRequestType { get; set; }
        }

        public class ExpectedOutput
        {
            public long ApprenticeshipId { get; set; }
            public long Ukprn { get; set; }
            public string EmployerName { get; set; }
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
            public DateTime? StopDate { get; set; }
            public DateTime? CreatedOn { get; set; }
            public long? ContinuationOfId { get; set; }
            public long? NewApprenticeshipId { get; set; }
        }
    }

    public class GetChangeOfEmployerQueryHandlerTestFixtures
    {
        public ProviderCommitmentsDbContext Db { get; set; }
        public GetChangeOfEmployerChainQueryHandler Handler { get; set; }
        
        public GetChangeOfEmployerQueryHandlerTestFixtures()
        {
            Db = new ProviderCommitmentsDbContext(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString(), b => b.EnableNullChecks(false)).Options);
            Handler = new GetChangeOfEmployerChainQueryHandler(
                new Lazy<ProviderCommitmentsDbContext>(() => Db));
        }

        public Task<GetChangeOfEmployerChainQueryResult> Handle(long apprenticeshipId)
        {
            var query = new GetChangeOfEmployerChainQuery(apprenticeshipId);
            return Handler.Handle(query, CancellationToken.None);
        }

        public void SetupCohort(CreateChangeOfEmployerChainDataCases.Input input)
        {
            // This line is required.
            // ReSharper disable once ObjectCreationAsStatement
            new UnitOfWorkContext();

            var autoFixture = new Fixture();

            var commitment = new Cohort(
                input.Ukprn,
                autoFixture.Create<long>(),
                input.CurrentEmployer.EmployerId,
                input.OriginatingParty,
                new UserInfo());

            Db.Cohorts.Add(commitment);
            Db.SaveChanges();

            var apprenticeship = new Apprenticeship
            {
                Id = input.ApprenticeshipId,
                CommitmentId = commitment.Id,
                StartDate = input.StartDate,
                ActualStartDate = input.ActualStartDate,
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
                        input.NewEmployer.EmployerId,
                        1000,
                        input.StartDate,
                        null,
                        null,
                        input.EmploymentEndDate,
                        null,
                        false,
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

            if (input.CurrentEmployer != null)
            {
                if (!Db.AccountLegalEntities.IgnoreQueryFilters().Any(ale => ale.Id == input.CurrentEmployer.EmployerId))
                {
                    var employer = new AccountLegalEntity
                    (
                        new Account(input.CurrentEmployer.EmployerId, "ABC123", "ABC123", "Test", DateTime.Now),
                        input.CurrentEmployer.EmployerId,
                        autoFixture.Create<long>(),
                        autoFixture.Create<string>(),
                        autoFixture.Create<string>(),
                        input.CurrentEmployer.EmployerName,
                        autoFixture.Create<OrganisationType>(),
                        autoFixture.Create<string>(),
                        DateTime.Now
                    );

                    Db.AccountLegalEntities.Add(employer);
                    Db.SaveChanges();
                }
            }

            if (input.NewEmployer != null)
            {
                if (!Db.AccountLegalEntities.IgnoreQueryFilters().Any(ale => ale.Id == input.NewEmployer.EmployerId))
                {
                    var employer = new AccountLegalEntity
                    (
                        new Account(input.NewEmployer.EmployerId, "ABC123", "ABC123", "Test", DateTime.Now),
                        input.NewEmployer.EmployerId,
                        autoFixture.Create<long>(),
                        autoFixture.Create<string>(),
                        autoFixture.Create<string>(),
                        input.NewEmployer.EmployerName,
                        autoFixture.Create<OrganisationType>(),
                        autoFixture.Create<string>(),
                        DateTime.Now
                    );

                    if (input.NewEmployer.IsDeleted)
                        employer.Delete(DateTime.Now);

                    Db.AccountLegalEntities.Add(employer);
                    Db.SaveChanges();
                }
            }
        }
    }
}