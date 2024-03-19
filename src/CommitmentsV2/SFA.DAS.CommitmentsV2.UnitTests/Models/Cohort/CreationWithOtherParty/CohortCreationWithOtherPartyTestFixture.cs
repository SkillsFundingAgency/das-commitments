using System;
using System.Linq;
using AutoFixture;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Domain.Extensions;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Services;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.UnitOfWork.Context;

namespace SFA.DAS.CommitmentsV2.UnitTests.Models.Cohort.CreationWithOtherParty
{
    public class CohortCreationWithOtherPartyTestFixture
    {
        private readonly Fixture _autoFixture = new Fixture();
        public Party CreatingParty { get; private set; }
        public string Message { get; private set; }
        public CommitmentsV2.Models.Cohort Cohort { get; private set; }
        public CommitmentsV2.Models.Provider Provider { get; private set; }
        public AccountLegalEntity AccountLegalEntity { get; private set; }
        public Account TransferSender { get; private set; }
        public Exception Exception { get; private set; }
        public UnitOfWorkContext UnitOfWorkContext { get; private set; }
        public UserInfo UserInfo { get; }
        public long? TransferSenderId { get; }
        public string TransferSenderName { get; }
        public int? PledgeApplicationId { get; }

        public CohortCreationWithOtherPartyTestFixture()
        {
            UnitOfWorkContext = new UnitOfWorkContext();
            Provider = new CommitmentsV2.Models.Provider(_autoFixture.Create<long>(), _autoFixture.Create<string>(),
                _autoFixture.Create<DateTime>(), _autoFixture.Create<DateTime>());

            var account = new Account(_autoFixture.Create<long>(), _autoFixture.Create<string>(),
                _autoFixture.Create<string>(), _autoFixture.Create<string>(), _autoFixture.Create<DateTime>());
            UserInfo = _autoFixture.Create<UserInfo>();

            AccountLegalEntity = new AccountLegalEntity(account,
                _autoFixture.Create<long>(),
                _autoFixture.Create<long>(),
                _autoFixture.Create<string>(),
                _autoFixture.Create<string>(),
                _autoFixture.Create<string>(),
                _autoFixture.Create<OrganisationType>(),
                _autoFixture.Create<string>(),
                _autoFixture.Create<DateTime>());

            TransferSenderId = _autoFixture.Create<long>();
            TransferSenderName = _autoFixture.Create<string>();
            TransferSender = new Account(TransferSenderId.Value, "XXX", "ZZZ", TransferSenderName, new DateTime());
            PledgeApplicationId = _autoFixture.Create<int?>();
        }

        public CohortCreationWithOtherPartyTestFixture WithCreatingParty(Party creatingParty)
        {
            CreatingParty = creatingParty;
            return this;
        }

        public CohortCreationWithOtherPartyTestFixture WithMessage(string message)
        {
            Message = message;
            return this;
        }

        public CohortCreationWithOtherPartyTestFixture WithNoTransferSender()
        {
            TransferSender = null;
            return this;
        }

        public void CreateCohort()
        {
            Exception = null;

            try
            {
                Cohort = new CommitmentsV2.Models.Cohort(Provider.UkPrn,
                    AccountLegalEntity.AccountId,
                    AccountLegalEntity.Id,
                    TransferSender?.Id,
                    PledgeApplicationId,
                    CreatingParty,
                    Message,
                    UserInfo);

                Cohort.TransferSender = TransferSender;
            }
            catch (ArgumentException ex)
            {
                Exception = ex;
            }
            catch (Exception ex)
            {
                Exception = ex;
            }
        }

        public void VerifyOriginator(Originator expectedOriginator)
        {
            Assert.That(Cohort.Originator, Is.EqualTo(expectedOriginator));
        }

        public void VerifyMessageIsAdded()
        {
            var createdBy = CreatingParty == Party.Employer ? 0 : 1;

            Assert.That(Cohort.Messages.Any(x =>
                x.Text == Message && x.Author == UserInfo.UserDisplayName && x.CreatedBy == createdBy), Is.True);
        }

        public void VerifyNoMessageIsAdded()
        {
            Assert.That(Cohort.Messages.Any(), Is.False);
        }

        public void VerifyException<T>()
        {
            Assert.That(Exception, Is.Not.Null);
            Assert.That(Exception, Is.InstanceOf<T>());
        }

        public void VerifyNoException()
        {
            Assert.That(Exception, Is.Null);
        }

        public void VerifyCohortIsNotDraft()
        {
            Assert.That(Cohort.IsDraft, Is.False);
        }

        public void VerifyCohortIsWithOtherParty()
        {
            Assert.That(Cohort.EditStatus, Is.EqualTo(CreatingParty.GetOtherParty().ToEditStatus()));
        }
        public void VerifyCohortHasTransferInformation()
        {
            Assert.That(Cohort.TransferSenderId, Is.EqualTo(TransferSenderId));
            Assert.That(Cohort.TransferSender.Name, Is.EqualTo(TransferSenderName));
        }

        public void VerifyCohortHasNoTransferInformation()
        {
            Assert.That(Cohort.TransferSenderId, Is.Null);
            Assert.That(Cohort.TransferSender, Is.Null);
        }

        public void VerifyCohortTracking()
        {
            Assert.That(UnitOfWorkContext.GetEvents().SingleOrDefault(x => x is EntityStateChangedEvent @event
                                                                                && @event.EntityType ==
                                                                                nameof(Cohort)), Is.Not.Null);
        }

        public void VerifyCohortAssignedToProviderEventIsPublished()
        {
            Assert.That(UnitOfWorkContext.GetEvents().SingleOrDefault(x => x is CohortAssignedToProviderEvent), Is.Not.Null);
        }
    }
}
