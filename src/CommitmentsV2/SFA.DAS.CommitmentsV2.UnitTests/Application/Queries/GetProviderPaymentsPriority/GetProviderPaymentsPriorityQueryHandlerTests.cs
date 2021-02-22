using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using SFA.DAS.UnitOfWork.Context;
using SFA.DAS.CommitmentsV2.Application.Queries.GetProviderPaymentsPriority;
using System.Collections;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Queries.GetProviderPaymentsPriority
{
    [TestFixture]
    [Parallelizable]
    public class GetProviderPaymentsPriorityQueryHandlerTests
    {
        [TestCaseSource(typeof(CreateProviderPaymentsDataCases))]
        public async Task Handle_WhenRequested_ThenItShouldReturnTheCurrentPriorityOrder(long accountId, CreateProviderPaymentsDataCases.Input[] inputs, CreateProviderPaymentsDataCases.ExpectedOutput[] expectedOutputs)
        {
            var fixture = new GetProviderPaymentsPriorityQueryHandlerTestFixtures();
            foreach(var input in inputs)
            {
                fixture.SetCohort(input.AccountId, input.ProviderId, input.ProviderName, input.ApprenticeshipAgreedOn, input.CohortApprovedOn, input.CohortFullyApproved, input.PriorityOrder);
            }

            var results = await fixture.Handle(accountId);
            Assert.IsTrue(TestHelpers.CompareHelper.AreEqualIgnoringTypes(results.PriorityItems, expectedOutputs));
        }
    }

    public class CreateProviderPaymentsDataCases : IEnumerable
    {
        public IEnumerator GetEnumerator()
        {
            var provider101 = new { ProviderId = 101, ProviderName = "Test101" };
            var provider102 = new { ProviderId = 102, ProviderName = "Test102" };
            var provider103 = new { ProviderId = 103, ProviderName = "Test103" };
            var provider201 = new { ProviderId = 201, ProviderName = "Test201" };
            
            // current custom provider payment priority ordered by P101, P102, P103
            yield return new object[] {
                111111,
                new Input[]
                {
                    new Input { AccountId = 111111, PriorityOrder = 1, ProviderId = provider101.ProviderId, ProviderName = provider101.ProviderName, ApprenticeshipAgreedOn = null, CohortApprovedOn = DateTime.Now.AddMonths(-3), CohortFullyApproved = true},
                    new Input { AccountId = 111111, PriorityOrder = 2, ProviderId = provider102.ProviderId, ProviderName = provider102.ProviderName, ApprenticeshipAgreedOn = null, CohortApprovedOn = DateTime.Now.AddMonths(-2), CohortFullyApproved = true},
                    new Input { AccountId = 111111, PriorityOrder = 3, ProviderId = provider103.ProviderId, ProviderName = provider103.ProviderName, ApprenticeshipAgreedOn = null, CohortApprovedOn = DateTime.Now.AddMonths(-1), CohortFullyApproved = true}
                },
                new ExpectedOutput[]
                {
                    new ExpectedOutput { ProviderId = provider101.ProviderId, ProviderName = provider101.ProviderName, PriorityOrder = 1},
                    new ExpectedOutput { ProviderId = provider102.ProviderId, ProviderName = provider102.ProviderName, PriorityOrder = 2},
                    new ExpectedOutput { ProviderId = provider103.ProviderId, ProviderName = provider103.ProviderName, PriorityOrder = 3}
                }
            };

            // no current custom provider payment priority ordered by P101, P102, P103
            yield return new object[] {
                222222,
                new Input[]
                {
                    new Input { AccountId = 222222, ProviderId = provider101.ProviderId, ProviderName = provider101.ProviderName, ApprenticeshipAgreedOn = null, CohortApprovedOn = DateTime.Now.AddMonths(-3), CohortFullyApproved = true, PriorityOrder = null},
                    new Input { AccountId = 222222, ProviderId = provider102.ProviderId, ProviderName = provider102.ProviderName, ApprenticeshipAgreedOn = null, CohortApprovedOn = DateTime.Now.AddMonths(-2), CohortFullyApproved = true, PriorityOrder = null},
                    new Input { AccountId = 222222, ProviderId = provider103.ProviderId, ProviderName = provider103.ProviderName, ApprenticeshipAgreedOn = null, CohortApprovedOn = DateTime.Now.AddMonths(-1), CohortFullyApproved = true, PriorityOrder = null},
                },
                new ExpectedOutput[]
                {
                    new ExpectedOutput { ProviderId = provider101.ProviderId, ProviderName = provider101.ProviderName, PriorityOrder = 1},
                    new ExpectedOutput { ProviderId = provider102.ProviderId, ProviderName = provider102.ProviderName, PriorityOrder = 2},
                    new ExpectedOutput { ProviderId = provider103.ProviderId, ProviderName = provider103.ProviderName, PriorityOrder = 3}
                }
            };

            // no current custom provider payment priority ordered by P102, P101, P103
            yield return new object[] {
                333333,
                new Input[]
                {
                    new Input { AccountId = 333333, ProviderId = provider101.ProviderId, ProviderName = provider101.ProviderName, ApprenticeshipAgreedOn = null, CohortApprovedOn = DateTime.Now.AddMonths(-2), CohortFullyApproved = true, PriorityOrder = null},
                    new Input { AccountId = 333333, ProviderId = provider102.ProviderId, ProviderName = provider102.ProviderName, ApprenticeshipAgreedOn = null, CohortApprovedOn = DateTime.Now.AddMonths(-3), CohortFullyApproved = true, PriorityOrder = null},
                    new Input { AccountId = 333333, ProviderId = provider103.ProviderId, ProviderName = provider103.ProviderName, ApprenticeshipAgreedOn = null, CohortApprovedOn = DateTime.Now.AddMonths(-1), CohortFullyApproved = true, PriorityOrder = null},
                },
                new ExpectedOutput[]
                {
                    new ExpectedOutput { ProviderId = provider102.ProviderId, ProviderName = provider102.ProviderName, PriorityOrder = 1},
                    new ExpectedOutput { ProviderId = provider101.ProviderId, ProviderName = provider101.ProviderName, PriorityOrder = 2},
                    new ExpectedOutput { ProviderId = provider103.ProviderId, ProviderName = provider103.ProviderName, PriorityOrder = 3}
                }
            };

            // no current custom provider payment priority ordered by P102, P101, P103
            yield return new object[] {
                444444,
                new Input[]
                {
                    new Input { AccountId = 444444, ProviderId = provider101.ProviderId, ProviderName = provider101.ProviderName, ApprenticeshipAgreedOn = DateTime.Now.AddMonths(-2), CohortApprovedOn = null, CohortFullyApproved = true, PriorityOrder = null},
                    new Input { AccountId = 444444, ProviderId = provider102.ProviderId, ProviderName = provider102.ProviderName, ApprenticeshipAgreedOn = null, CohortApprovedOn = DateTime.Now.AddMonths(-3), CohortFullyApproved = true, PriorityOrder = null},
                    new Input { AccountId = 444444, ProviderId = provider103.ProviderId, ProviderName = provider103.ProviderName, ApprenticeshipAgreedOn = DateTime.Now.AddMonths(-1), CohortApprovedOn = null, CohortFullyApproved = true, PriorityOrder = null},
                },
                new ExpectedOutput[]
                {
                    new ExpectedOutput { ProviderId = provider102.ProviderId, ProviderName = provider102.ProviderName, PriorityOrder = 1},
                    new ExpectedOutput { ProviderId = provider101.ProviderId, ProviderName = provider101.ProviderName, PriorityOrder = 2},
                    new ExpectedOutput { ProviderId = provider103.ProviderId, ProviderName = provider103.ProviderName, PriorityOrder = 3}
                }
            };

            // no current custom provider payment priority ordered by P102 (Not Approved), P101, P103
            yield return new object[] {
                555555,
                new Input[]
                {
                    new Input { AccountId = 555555, ProviderId = provider101.ProviderId, ProviderName = provider101.ProviderName, ApprenticeshipAgreedOn = null, CohortApprovedOn = DateTime.Now.AddMonths(-2), CohortFullyApproved = true, PriorityOrder = null},
                    new Input { AccountId = 555555, ProviderId = provider102.ProviderId, ProviderName = provider102.ProviderName, ApprenticeshipAgreedOn = null, CohortApprovedOn = DateTime.Now.AddMonths(-3), CohortFullyApproved = false, PriorityOrder = null},
                    new Input { AccountId = 555555, ProviderId = provider103.ProviderId, ProviderName = provider103.ProviderName, ApprenticeshipAgreedOn = null, CohortApprovedOn = DateTime.Now.AddMonths(-1), CohortFullyApproved = true, PriorityOrder = null},
                },
                new ExpectedOutput[]
                {
                    new ExpectedOutput { ProviderId = provider101.ProviderId, ProviderName = provider101.ProviderName, PriorityOrder = 1},
                    new ExpectedOutput { ProviderId = provider103.ProviderId, ProviderName = provider103.ProviderName, PriorityOrder = 2}
                }
            };

            // partial current custom provider payment priority ordered by P102, P101, P103 (With Existing Priority 1)
            yield return new object[] {
                666666,
                new Input[]
                {
                    new Input { AccountId = 666666, ProviderId = provider101.ProviderId, ProviderName = provider101.ProviderName, ApprenticeshipAgreedOn = null, CohortApprovedOn = DateTime.Now.AddMonths(-2), CohortFullyApproved = true, PriorityOrder = null},
                    new Input { AccountId = 666666, ProviderId = provider102.ProviderId, ProviderName = provider102.ProviderName, ApprenticeshipAgreedOn = null, CohortApprovedOn = DateTime.Now.AddMonths(-3), CohortFullyApproved = true, PriorityOrder = null},
                    new Input { AccountId = 666666, ProviderId = provider103.ProviderId, ProviderName = provider103.ProviderName, ApprenticeshipAgreedOn = null, CohortApprovedOn = DateTime.Now.AddMonths(-1), CohortFullyApproved = true, PriorityOrder = 1},
                },
                new ExpectedOutput[]
                {
                    new ExpectedOutput { ProviderId = provider103.ProviderId, ProviderName = provider103.ProviderName, PriorityOrder = 1},
                    new ExpectedOutput { ProviderId = provider102.ProviderId, ProviderName = provider102.ProviderName, PriorityOrder = 2},
                    new ExpectedOutput { ProviderId = provider101.ProviderId, ProviderName = provider101.ProviderName, PriorityOrder = 3}
                }
            };

            // current custom provider payment priority ordered by P101, P102, P103 (With earlier date for multiple 103 which is ignored)
            yield return new object[] {
                777777,
                new Input[]
                {
                    new Input { AccountId = 777777, PriorityOrder = 1, ProviderId = provider101.ProviderId, ProviderName = provider101.ProviderName, ApprenticeshipAgreedOn = null, CohortApprovedOn = DateTime.Now.AddMonths(-3), CohortFullyApproved = true},
                    new Input { AccountId = 777777, PriorityOrder = 2, ProviderId = provider102.ProviderId, ProviderName = provider102.ProviderName, ApprenticeshipAgreedOn = null, CohortApprovedOn = DateTime.Now.AddMonths(-2), CohortFullyApproved = true},
                    new Input { AccountId = 777777, PriorityOrder = 3, ProviderId = provider103.ProviderId, ProviderName = provider103.ProviderName, ApprenticeshipAgreedOn = null, CohortApprovedOn = DateTime.Now.AddMonths(-1), CohortFullyApproved = true},
                    new Input { AccountId = 777777, PriorityOrder = 3, ProviderId = provider103.ProviderId, ProviderName = provider103.ProviderName, ApprenticeshipAgreedOn = null, CohortApprovedOn = DateTime.Now.AddMonths(-4), CohortFullyApproved = true},
                    new Input { AccountId = 888888, PriorityOrder = 1, ProviderId = provider101.ProviderId, ProviderName = provider101.ProviderName, ApprenticeshipAgreedOn = null, CohortApprovedOn = DateTime.Now.AddMonths(-1), CohortFullyApproved = true},
                    new Input { AccountId = 888888, PriorityOrder = 2, ProviderId = provider102.ProviderId, ProviderName = provider102.ProviderName, ApprenticeshipAgreedOn = null, CohortApprovedOn = DateTime.Now.AddMonths(-2), CohortFullyApproved = true},
                    new Input { AccountId = 999999, PriorityOrder = 1, ProviderId = provider201.ProviderId, ProviderName = provider201.ProviderName, ApprenticeshipAgreedOn = null, CohortApprovedOn = DateTime.Now.AddMonths(-3), CohortFullyApproved = true},
                },
                new ExpectedOutput[]
                {
                    new ExpectedOutput { ProviderId = provider101.ProviderId, ProviderName = provider101.ProviderName, PriorityOrder = 1},
                    new ExpectedOutput { ProviderId = provider102.ProviderId, ProviderName = provider102.ProviderName, PriorityOrder = 2},
                    new ExpectedOutput { ProviderId = provider103.ProviderId, ProviderName = provider103.ProviderName, PriorityOrder = 3}
                }
            };

            // no custom provider payment priority ordered by P103 (With earlier date for multiple 103 which is used), P101, P102, P103 (With later date for multiple 103 which is ignored)
            yield return new object[] {
                888888,
                new Input[]
                {
                    new Input { AccountId = 888888, PriorityOrder = null, ProviderId = provider101.ProviderId, ProviderName = provider101.ProviderName, ApprenticeshipAgreedOn = null, CohortApprovedOn = DateTime.Now.AddMonths(-3), CohortFullyApproved = true},
                    new Input { AccountId = 888888, PriorityOrder = null, ProviderId = provider102.ProviderId, ProviderName = provider102.ProviderName, ApprenticeshipAgreedOn = null, CohortApprovedOn = DateTime.Now.AddMonths(-2), CohortFullyApproved = true},
                    new Input { AccountId = 888888, PriorityOrder = null, ProviderId = provider103.ProviderId, ProviderName = provider103.ProviderName, ApprenticeshipAgreedOn = null, CohortApprovedOn = DateTime.Now.AddMonths(-1), CohortFullyApproved = true},
                    new Input { AccountId = 888888, PriorityOrder = null, ProviderId = provider103.ProviderId, ProviderName = provider103.ProviderName, ApprenticeshipAgreedOn = null, CohortApprovedOn = DateTime.Now.AddMonths(-4), CohortFullyApproved = true},
                    new Input { AccountId = 999999, PriorityOrder = 1, ProviderId = provider101.ProviderId, ProviderName = provider101.ProviderName, ApprenticeshipAgreedOn = null, CohortApprovedOn = DateTime.Now.AddMonths(-1), CohortFullyApproved = true},
                    new Input { AccountId = 999999, PriorityOrder = 2, ProviderId = provider102.ProviderId, ProviderName = provider102.ProviderName, ApprenticeshipAgreedOn = null, CohortApprovedOn = DateTime.Now.AddMonths(-2), CohortFullyApproved = true},
                    new Input { AccountId = 111111, PriorityOrder = 1, ProviderId = provider201.ProviderId, ProviderName = provider201.ProviderName, ApprenticeshipAgreedOn = null, CohortApprovedOn = DateTime.Now.AddMonths(-3), CohortFullyApproved = true},
                },
                new ExpectedOutput[]
                {
                    new ExpectedOutput { ProviderId = provider103.ProviderId, ProviderName = provider103.ProviderName, PriorityOrder = 1},
                    new ExpectedOutput { ProviderId = provider101.ProviderId, ProviderName = provider101.ProviderName, PriorityOrder = 2},
                    new ExpectedOutput { ProviderId = provider102.ProviderId, ProviderName = provider102.ProviderName, PriorityOrder = 3}, 
                }
            };

            // partial custom provider payment priority ordered by P102, P201, P103 (With earlier date for multiple 103 which is used), P101, P103 (With later date for multiple 103 which is ignored)
            yield return new object[] {
                999999,
                new Input[]
                {
                    new Input { AccountId = 999999, PriorityOrder = null, ProviderId = provider101.ProviderId, ProviderName = provider101.ProviderName, ApprenticeshipAgreedOn = null, CohortApprovedOn = DateTime.Now.AddMonths(-3), CohortFullyApproved = true},
                    new Input { AccountId = 999999, PriorityOrder = 1, ProviderId = provider102.ProviderId, ProviderName = provider102.ProviderName, ApprenticeshipAgreedOn = null, CohortApprovedOn = DateTime.Now.AddMonths(-2), CohortFullyApproved = true},
                    new Input { AccountId = 999999, PriorityOrder = null, ProviderId = provider103.ProviderId, ProviderName = provider103.ProviderName, ApprenticeshipAgreedOn = null, CohortApprovedOn = DateTime.Now.AddMonths(-1), CohortFullyApproved = true},
                    new Input { AccountId = 999999, PriorityOrder = null, ProviderId = provider103.ProviderId, ProviderName = provider103.ProviderName, ApprenticeshipAgreedOn = null, CohortApprovedOn = DateTime.Now.AddMonths(-4), CohortFullyApproved = true},
                    new Input { AccountId = 999999, PriorityOrder = 2, ProviderId = provider201.ProviderId, ProviderName = provider201.ProviderName, ApprenticeshipAgreedOn = null, CohortApprovedOn = DateTime.Now.AddMonths(-1), CohortFullyApproved = true},
                    new Input { AccountId = 111111, PriorityOrder = 1, ProviderId = provider101.ProviderId, ProviderName = provider101.ProviderName, ApprenticeshipAgreedOn = null, CohortApprovedOn = DateTime.Now.AddMonths(-1), CohortFullyApproved = true},
                    new Input { AccountId = 111111, PriorityOrder = 2, ProviderId = provider102.ProviderId, ProviderName = provider102.ProviderName, ApprenticeshipAgreedOn = null, CohortApprovedOn = DateTime.Now.AddMonths(-2), CohortFullyApproved = true},
                    new Input { AccountId = 222222, PriorityOrder = 1, ProviderId = provider201.ProviderId, ProviderName = provider201.ProviderName, ApprenticeshipAgreedOn = null, CohortApprovedOn = DateTime.Now.AddMonths(-3), CohortFullyApproved = true},
                },
                new ExpectedOutput[]
                {
                    new ExpectedOutput { ProviderId = provider102.ProviderId, ProviderName = provider102.ProviderName, PriorityOrder = 1},
                    new ExpectedOutput { ProviderId = provider201.ProviderId, ProviderName = provider201.ProviderName, PriorityOrder = 2},
                    new ExpectedOutput { ProviderId = provider103.ProviderId, ProviderName = provider103.ProviderName, PriorityOrder = 3},
                    new ExpectedOutput { ProviderId = provider101.ProviderId, ProviderName = provider101.ProviderName, PriorityOrder = 4},
                }
            };

            // partial custom provider payment priority ordered by P102, P201, P103 (With earlier date for multiple 103 which is used), P101, P103 (With later date for multiple 103 which is ignored)
            yield return new object[] {
                111222,
                new Input[]
                {
                    new Input { AccountId = 111222, PriorityOrder = null, ProviderId = provider101.ProviderId, ProviderName = provider101.ProviderName, ApprenticeshipAgreedOn = null, CohortApprovedOn = DateTime.Now.AddMonths(-3), CohortFullyApproved = true},
                    new Input { AccountId = 111222, PriorityOrder = null, ProviderId = provider101.ProviderId, ProviderName = provider101.ProviderName, ApprenticeshipAgreedOn = null, CohortApprovedOn = DateTime.Now.AddMonths(-2), CohortFullyApproved = true},
                    new Input { AccountId = 111222, PriorityOrder = null, ProviderId = provider101.ProviderId, ProviderName = provider101.ProviderName, ApprenticeshipAgreedOn = null, CohortApprovedOn = DateTime.Now.AddMonths(-1), CohortFullyApproved = true},
                    new Input { AccountId = 111222, PriorityOrder = 1, ProviderId = provider102.ProviderId, ProviderName = provider102.ProviderName, ApprenticeshipAgreedOn = null, CohortApprovedOn = DateTime.Now.AddMonths(-2), CohortFullyApproved = true},
                    new Input { AccountId = 111222, PriorityOrder = null, ProviderId = provider103.ProviderId, ProviderName = provider103.ProviderName, ApprenticeshipAgreedOn = null, CohortApprovedOn = DateTime.Now.AddMonths(-1), CohortFullyApproved = true},
                    new Input { AccountId = 111222, PriorityOrder = null, ProviderId = provider103.ProviderId, ProviderName = provider103.ProviderName, ApprenticeshipAgreedOn = null, CohortApprovedOn = DateTime.Now.AddMonths(-4), CohortFullyApproved = true},
                    new Input { AccountId = 111222, PriorityOrder = null, ProviderId = provider103.ProviderId, ProviderName = provider103.ProviderName, ApprenticeshipAgreedOn = null, CohortApprovedOn = DateTime.Now.AddMonths(-2), CohortFullyApproved = true},
                    new Input { AccountId = 111222, PriorityOrder = null, ProviderId = provider103.ProviderId, ProviderName = provider103.ProviderName, ApprenticeshipAgreedOn = null, CohortApprovedOn = DateTime.Now.AddMonths(-3), CohortFullyApproved = true},
                    new Input { AccountId = 111222, PriorityOrder = 2, ProviderId = provider201.ProviderId, ProviderName = provider201.ProviderName, ApprenticeshipAgreedOn = null, CohortApprovedOn = DateTime.Now.AddMonths(-1), CohortFullyApproved = true},
                    new Input { AccountId = 111222, PriorityOrder = 2, ProviderId = provider201.ProviderId, ProviderName = provider201.ProviderName, ApprenticeshipAgreedOn = null, CohortApprovedOn = DateTime.Now.AddMonths(-2), CohortFullyApproved = true},
                    new Input { AccountId = 111222, PriorityOrder = 2, ProviderId = provider201.ProviderId, ProviderName = provider201.ProviderName, ApprenticeshipAgreedOn = null, CohortApprovedOn = DateTime.Now.AddMonths(-3), CohortFullyApproved = true},
                    new Input { AccountId = 888888, PriorityOrder = 1, ProviderId = provider101.ProviderId, ProviderName = provider101.ProviderName, ApprenticeshipAgreedOn = null, CohortApprovedOn = DateTime.Now.AddMonths(-1), CohortFullyApproved = true},
                    new Input { AccountId = 888888, PriorityOrder = 2, ProviderId = provider102.ProviderId, ProviderName = provider102.ProviderName, ApprenticeshipAgreedOn = null, CohortApprovedOn = DateTime.Now.AddMonths(-2), CohortFullyApproved = true},
                    new Input { AccountId = 999999, PriorityOrder = 1, ProviderId = provider201.ProviderId, ProviderName = provider201.ProviderName, ApprenticeshipAgreedOn = null, CohortApprovedOn = DateTime.Now.AddMonths(-3), CohortFullyApproved = true},
                },
                new ExpectedOutput[]
                {
                    new ExpectedOutput { ProviderId = provider102.ProviderId, ProviderName = provider102.ProviderName, PriorityOrder = 1},
                    new ExpectedOutput { ProviderId = provider201.ProviderId, ProviderName = provider201.ProviderName, PriorityOrder = 2},
                    new ExpectedOutput { ProviderId = provider103.ProviderId, ProviderName = provider103.ProviderName, PriorityOrder = 3},
                    new ExpectedOutput { ProviderId = provider101.ProviderId, ProviderName = provider101.ProviderName, PriorityOrder = 4},
                }
            };
        }

        public class Input
        {
            public long AccountId { get; set; }
            public long ProviderId { get; set; }
            public string ProviderName { get; set; }
            public DateTime? ApprenticeshipAgreedOn { get; set; } 
            public DateTime? CohortApprovedOn { get; set; }
            public bool CohortFullyApproved { get; set; }
            public int? PriorityOrder { get; set; }
        }

        public class ExpectedOutput
        {
            public long ProviderId { get; set; }
            public string ProviderName { get; set; }
            public int PriorityOrder { get; set; }
        }
    }

    public class GetProviderPaymentsPriorityQueryHandlerTestFixtures
    {
        public ProviderCommitmentsDbContext Db { get; set; }
        public GetProviderPaymentsPriorityQueryHandler Handler { get; set; }

        public GetProviderPaymentsPriorityQueryHandlerTestFixtures()
        {
            Db = new ProviderCommitmentsDbContext(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
            Handler = new GetProviderPaymentsPriorityQueryHandler(
                new Lazy<ProviderCommitmentsDbContext>(() => Db));
        }

        public Task<GetProviderPaymentsPriorityQueryResult> Handle(long accountId)
        {
            var query = new GetProviderPaymentsPriorityQuery(accountId);
            return Handler.Handle(query, CancellationToken.None);
        }

        public void SetCohort(long accountId, long providerId, string providerName, DateTime? agreedOn, DateTime? approvedOn, bool cohortApproved, int? priorityOrder)
        {
            // This line is required.
            // ReSharper disable once ObjectCreationAsStatement
            new UnitOfWorkContext();

            var autoFixture = new Fixture();

            var commitment = new Cohort(
                providerId, 
                accountId,
                autoFixture.Create<long>(),
                Party.Employer,
                new UserInfo());

            commitment.EmployerAndProviderApprovedOn = approvedOn;

            Db.Cohorts.Add(commitment);
            Db.SaveChanges();

            if (cohortApproved)
            {
                commitment.EditStatus = EditStatus.Both;
                commitment.TransferSenderId = null;
                commitment.TransferApprovalStatus = TransferApprovalStatus.Approved;
                Db.SaveChanges();
            }

            var apprenticeship = new Apprenticeship
            {
                CommitmentId = commitment.Id,
                AgreedOn = agreedOn
            };
                
            Db.Apprenticeships.Add(apprenticeship);
            Db.SaveChanges();

            if (!Db.Providers.Any(p => p.UkPrn == providerId))
            {
                var provider = new Provider()
                {
                    UkPrn = providerId,
                    Name = providerName
                };

                Db.Providers.Add(provider);
                Db.SaveChanges();
            }

            if (priorityOrder.HasValue)
            {
                if (!Db.CustomProviderPaymentPriorities.Any(p => p.EmployerAccountId == accountId && p.ProviderId == providerId))
                {
                    var customProviderPaymentPriority = new CustomProviderPaymentPriority
                    {
                        ProviderId = providerId,
                        EmployerAccountId = accountId,
                        PriorityOrder = priorityOrder.Value
                    };

                    Db.CustomProviderPaymentPriorities.Add(customProviderPaymentPriority);
                    Db.SaveChanges();
                }
            }
        }
    }
}