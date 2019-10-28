using System;
using System.Threading.Tasks;
using NServiceBus;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.EmployerAccounts.Messages.Events;
using SFA.DAS.EmployerAccounts.Types.Models;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.TestHarness
{
    public class TestHarness
    {
        private readonly IMessageSession _publisher;

        public TestHarness(IMessageSession publisher)
        {
            _publisher = publisher;
        }

        public async Task Run()
        {
            long accountId = 1001;
            long accountLegalEntityId = 2001;
            long cohortId = 186091;

            ConsoleKey key = ConsoleKey.Escape;

            while (key != ConsoleKey.X)
            {
                Console.Clear();
                Console.WriteLine("Test Options");
                Console.WriteLine("------------");
                Console.WriteLine("A - CreateAccountEvent");
                Console.WriteLine("B - ChangedAccountNameEvent");
                Console.WriteLine("C - AddedLegalEntityEvent");
                Console.WriteLine("D - UpdatedLegalEntityEvent");
                Console.WriteLine("E - RemovedLegalEntityEvent");
                Console.WriteLine("F - DraftApprenticeshipCreatedEvent");
                Console.WriteLine("G - BulkUploadIntoCohortCompletedEvent");
                Console.WriteLine("H - CohortAssignedToProviderEvent");
                Console.WriteLine("I - CohortTransferApprovalRequestedEvent");
                Console.WriteLine("M - ApprovedCohortReturnedToProviderEvent");
                Console.WriteLine("N - CohortApprovedByEmployer");
                Console.WriteLine("X - Exit");
                Console.WriteLine("Press [Key] for Test Option");
                key = Console.ReadKey().Key;


                try
                {
                    switch (key)
                    {
                        case ConsoleKey.A:
                            await _publisher.Publish(new CreatedAccountEvent { AccountId = accountId, Created = DateTime.Now, HashedId = "HPRIV", PublicHashedId = "PUBH", Name = "My Test", UserName = "Tester", UserRef = Guid.NewGuid() });
                            Console.WriteLine();
                            Console.WriteLine($"Published CreatedAccountEvent");
                            break;
                        case ConsoleKey.B:
                            await _publisher.Publish(new ChangedAccountNameEvent { AccountId = accountId, Created = DateTime.Now, CurrentName = "My Test new", PreviousName = "My Test", HashedAccountId = "PUBH", UserName = "Tester", UserRef = Guid.NewGuid() });
                            Console.WriteLine();
                            Console.WriteLine($"Published ChangedAccountNameEvent");
                            break;
                        case ConsoleKey.C:
                            await _publisher.Publish(new AddedLegalEntityEvent { AccountId = accountId, Created = DateTime.Now, AccountLegalEntityId = accountLegalEntityId,
                                OrganisationType = OrganisationType.Charities, OrganisationReferenceNumber = "MyLegalEntityId", OrganisationAddress = "My Address",
                                AccountLegalEntityPublicHashedId = "ABCD", AgreementId = 9898, LegalEntityId = 75263,
                                OrganisationName = "My Legal Entity",  UserName = "Tester", UserRef = Guid.NewGuid() });
                            Console.WriteLine();
                            Console.WriteLine($"Published AddedLegalEntityEvent");
                            break;
                        case ConsoleKey.D:
                            await _publisher.Publish(new UpdatedLegalEntityEvent { AccountLegalEntityId = accountLegalEntityId, Created = DateTime.Now, Name = "TEST", OrganisationName = "OName", UserName = "Tester", UserRef = Guid.NewGuid() });
                            Console.WriteLine();
                            Console.WriteLine($"Published UpdatedLegalEntityEvent");
                            break;
                        case ConsoleKey.E:
                            await _publisher.Publish(new RemovedLegalEntityEvent { AccountLegalEntityId = accountLegalEntityId, Created = DateTime.Now, AccountId = accountId, OrganisationName = "OName", LegalEntityId = 75263, AgreementId = 9898, UserName = "Tester", UserRef = Guid.NewGuid() });
                            Console.WriteLine();
                            Console.WriteLine($"Published RemovedLegalEntityEvent");
                            break;
                        case ConsoleKey.F:
                            await _publisher.Publish(new DraftApprenticeshipCreatedEvent(111111, 222222, "AAA111", Guid.NewGuid(), DateTime.UtcNow));
                            Console.WriteLine();
                            Console.WriteLine($"Published {nameof(DraftApprenticeshipCreatedEvent)}");
                            break;
                        case ConsoleKey.G:
                            await _publisher.Publish(new BulkUploadIntoCohortCompletedEvent { CohortId = cohortId, ProviderId = 10010, NumberOfApprentices = 0, UploadedOn =  DateTime.Now});
                            Console.WriteLine();
                            Console.WriteLine($"Published {nameof(DraftApprenticeshipCreatedEvent)}");
                            break;
                        case ConsoleKey.H:
                            await _publisher.Publish(new CohortAssignedToProviderEvent(186091, DateTime.Now));
                            Console.WriteLine();
                            Console.WriteLine($"Published {nameof(CohortAssignedToProviderEvent)}");
                            break;
                        case ConsoleKey.I:
                            await _publisher.Publish(new CohortTransferApprovalRequestedEvent(186091, DateTime.Now));
                            Console.WriteLine();
                            Console.WriteLine($"Published {nameof(CohortTransferApprovalRequestedEvent)}");
                            break;
                        case ConsoleKey.M:
                            await _publisher.Publish(new ApprovedCohortReturnedToProviderEvent(cohortId, DateTime.Now));
                            Console.WriteLine();
                            Console.WriteLine($"Published {nameof(ApprovedCohortReturnedToProviderEvent)}");
                            break;
                        case ConsoleKey.N:
                            await _publisher.Publish(new CohortApprovedByEmployerEvent(cohortId, DateTime.Now));
                            Console.WriteLine();
                            Console.WriteLine($"Published {nameof(CohortApprovedByEmployerEvent)}");
                            break;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    Console.WriteLine();
                }

                if (key == ConsoleKey.X) break;

                Console.WriteLine();
                Console.WriteLine("Press anykey to return to menu");
                Console.ReadKey();
            }
        }
    }
}
