using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NServiceBus;
using SFA.DAS.ApprenticeCommitments.Messages.Events;
using SFA.DAS.CommitmentsV2.Messages.Commands;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.EmployerAccounts.Messages.Events;
using SFA.DAS.EmployerAccounts.Types.Models;
using SFA.DAS.EmployerFinance.Messages.Events;
using SFA.DAS.Payments.ProviderPayments.Messages;

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
            long accountLegalEntityId = 2061;
            long cohortId = 186091;
            UserInfo userInfo = new UserInfo { UserDisplayName = "Paul Graham", UserEmail = "paul.graham@test.com", UserId = "PG"};

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
                Console.WriteLine("J - ApprovedCohortReturnedToProviderEvent");
                Console.WriteLine("K - CohortApprovedByEmployer");
                Console.WriteLine("L - SendEmailToEmployerCommand");
                Console.WriteLine("M - RunHealthCheckCommand");
                Console.WriteLine("N - RecordedAct1CompletionPayment Event");
                Console.WriteLine("O - CohortDeletedEvent");
                Console.WriteLine("P - ApproveTransferRequestCommand");
                Console.WriteLine("Q - RejectTransferRequestCommand");
                Console.WriteLine("R - ApprenticeshipEmailAddressConfirmedEvent Event");
                Console.WriteLine("S - LevyAddedToAccount");
                Console.WriteLine("T - CohortWithChangeOfPartyCreatedEvent Event");
                Console.WriteLine("U - ApprenticeshipPausedEvent Event");
                Console.WriteLine("V - ApprenticeshipConfirmationCommencedEvent Event");
                Console.WriteLine("W - ApprenticeshipConfirmedEvent Event");
                Console.WriteLine("Y - ApprenticeshipEmailAddressChangedEvent Event");
                Console.WriteLine("Z - ApprenticeshipStopDateChangedEvent Event");
                Console.WriteLine("1 - ChangeOfPartyRequestCreatedEvent Event");
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
                            await _publisher.Publish(new CohortTransferApprovalRequestedEvent(186091, DateTime.Now, Party.Employer));
                            Console.WriteLine();
                            Console.WriteLine($"Published {nameof(CohortTransferApprovalRequestedEvent)}");
                            break;
                        case ConsoleKey.J:
                            await _publisher.Publish(new ApprovedCohortReturnedToProviderEvent(cohortId, DateTime.Now));
                            Console.WriteLine();
                            Console.WriteLine($"Published {nameof(ApprovedCohortReturnedToProviderEvent)}");
                            break;
                        case ConsoleKey.K:
                            await _publisher.Publish(new CohortApprovedByEmployerEvent(cohortId, DateTime.Now));
                            Console.WriteLine();
                            Console.WriteLine($"Published {nameof(CohortApprovedByEmployerEvent)}");
                            break;
                        case ConsoleKey.L:
                            await _publisher.Send(new SendEmailToEmployerCommand(10003, "ABCDE", new Dictionary<string, string>(), "Test@test.com"), new SendOptions());
                            Console.WriteLine();
                            Console.WriteLine($"Sent {nameof(SendEmailToEmployerCommand)}");
                            break;
                        case ConsoleKey.M:
                            await _publisher.Send(new RunHealthCheckCommand(), new SendOptions());
                            Console.WriteLine();
                            Console.WriteLine($"Sent {nameof(RunHealthCheckCommand)}");
                            break;
                        case ConsoleKey.N:
                            await _publisher.Publish(new RecordedAct1CompletionPayment { ApprenticeshipId = 1, EventTime = DateTimeOffset.UtcNow });
                            Console.WriteLine();
                            Console.WriteLine($"Published {nameof(RecordedAct1CompletionPayment)}");
                            break;
                        case ConsoleKey.O:
                            await _publisher.Publish(new CohortDeletedEvent(cohortId, 22222, 33333, Party.None, DateTime.Now));
                            Console.WriteLine();
                            Console.WriteLine($"Published {nameof(CohortDeletedEvent)}");
                            break;
                        case ConsoleKey.P:
                            await _publisher.Send(new ApproveTransferRequestCommand(10004, DateTime.UtcNow, userInfo), new SendOptions());
                            Console.WriteLine();
                            Console.WriteLine($"Sent {nameof(ApproveTransferRequestCommand)}");
                            break;
                        case ConsoleKey.Q:
                            await _publisher.Send(new RejectTransferRequestCommand(10004, DateTime.UtcNow, userInfo), new SendOptions());
                            Console.WriteLine();
                            Console.WriteLine($"Sent {nameof(RejectTransferRequestCommand)}");
                            break;

                        case ConsoleKey.R:
                            await _publisher.Publish(new ApprenticeshipEmailAddressConfirmedEvent()
                            {
                                ApprenticeId = Guid.NewGuid(),
                                CommitmentsApprenticeshipId = 40002,
                            });
                            Console.WriteLine();
                            Console.WriteLine($"Sent {nameof(ApprenticeshipEmailAddressConfirmedEvent)}");
                            break;

                        case ConsoleKey.S:
                            await _publisher.Publish(new LevyAddedToAccount { AccountId = accountId, Amount = 10, Created = DateTime.UtcNow });
                            Console.WriteLine();
                            Console.WriteLine($"Sent {nameof(LevyAddedToAccount)}");
                            break;
                        case ConsoleKey.T:
                            await _publisher.Publish(new CohortWithChangeOfPartyCreatedEvent (10006, 1, Party.Provider, DateTime.UtcNow, new UserInfo()));
                            Console.WriteLine();
                            Console.WriteLine($"Sent {nameof(CohortWithChangeOfPartyCreatedEvent)}");
                            break;
                        case ConsoleKey.U:
                            await _publisher.Publish(new ApprenticeshipPausedEvent() { ApprenticeshipId = 80024, PausedOn = DateTime.Now });
                            Console.WriteLine();
                            Console.WriteLine($"Sent {nameof(ApprenticeshipPausedEvent)}");
                            break;
                        case ConsoleKey.V:
                            await _publisher.Publish(new ApprenticeshipConfirmationCommencedEvent
                            {
                                //ApprenticeId = Guid.NewGuid(),
                                CommitmentsApprenticeshipId = 40002,
                                CommitmentsApprovedOn = DateTime.Now.AddDays(-1),
                                ConfirmationOverdueOn = DateTime.Now.AddDays(13)
                            });
                            Console.WriteLine();
                            Console.WriteLine($"Sent {nameof(ApprenticeshipConfirmationCommencedEvent)}");
                            break;
                        case ConsoleKey.W:
                            await _publisher.Publish(new ApprenticeshipConfirmationConfirmedEvent()
                            {
                                //ApprenticeId = Guid.NewGuid(),
                                CommitmentsApprenticeshipId = 40002,
                                CommitmentsApprovedOn = DateTime.Now.AddDays(-1),
                                ConfirmedOn = DateTime.Now
                            });
                            Console.WriteLine();
                            Console.WriteLine($"Sent {nameof(ApprenticeshipConfirmationCommencedEvent)}");
                            break;
                        case ConsoleKey.Y:
                            await _publisher.Publish(new ApprenticeshipEmailAddressChangedEvent()
                            {
                                ApprenticeId = Guid.NewGuid(),
                                CommitmentsApprenticeshipId = 40002,
                            });
                            Console.WriteLine();
                            Console.WriteLine($"Sent {nameof(ApprenticeshipEmailAddressChangedEvent)}");
                            break;
                        case ConsoleKey.Z:
                            await _publisher.Publish(new ApprenticeshipStopDateChangedEvent()
                            {
                                ApprenticeshipId = 113938,
                                StopDate = DateTime.Now.AddMonths(-1),
                            });
                            Console.WriteLine();
                            Console.WriteLine($"Sent {nameof(ApprenticeshipStopDateChangedEvent)}");
                            break; 
                        case ConsoleKey.D1:
                            await _publisher.Publish(new ChangeOfPartyRequestCreatedEvent(12345, new UserInfo(), false));
                            Console.WriteLine();
                            Console.WriteLine($"Sent {nameof(ChangeOfPartyRequestCreatedEvent)}");
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
                Console.WriteLine("Press any key to return to menu");
                Console.ReadKey();
            }
        }
    }
}
