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
            Assert.AreEqual(expectedOriginator, Cohort.Originator);
        }

        public void VerifyMessageIsAdded()
        {
            var createdBy = CreatingParty == Party.Employer ? 0 : 1;

            Assert.IsTrue(Cohort.Messages.Any(x =>
                x.Text == Message && x.Author == UserInfo.UserDisplayName && x.CreatedBy == createdBy));
        }

        public void VerifyNoMessageIsAdded()
        {
            Assert.IsFalse(Cohort.Messages.Any());
        }

        public void VerifyException<T>()
        {
            Assert.IsNotNull(Exception);
            Assert.IsInstanceOf<T>(Exception);
        }

        public void VerifyNoException()
        {
            Assert.IsNull(Exception);
        }

        public void VerifyCohortIsNotDraft()
        {
            Assert.IsFalse(Cohort.IsDraft);
        }

        public void VerifyCohortIsWithOtherParty()
        {
            Assert.AreEqual(CreatingParty.GetOtherParty().ToEditStatus(), Cohort.EditStatus);
        }
        public void VerifyCohortHasTransferInformation()
        {
            Assert.AreEqual(TransferSenderId, Cohort.TransferSenderId);
            Assert.AreEqual(TransferSenderName, Cohort.TransferSender.Name);
        }

        public void VerifyCohortHasNoTransferInformation()
        {
            Assert.IsNull(Cohort.TransferSenderId);
            Assert.IsNull(Cohort.TransferSender);
        }

        public void VerifyCohortTracking()
        {
            Assert.IsNotNull(UnitOfWorkContext.GetEvents().SingleOrDefault(x => x is EntityStateChangedEvent @event
                                                                                && @event.EntityType ==
                                                                                nameof(Cohort)));
        }

        public void VerifyCohortAssignedToProviderEventIsPublished()
        {
            Assert.IsNotNull(UnitOfWorkContext.GetEvents().SingleOrDefault(x => x is CohortAssignedToProviderEvent));
        }
    }
}
