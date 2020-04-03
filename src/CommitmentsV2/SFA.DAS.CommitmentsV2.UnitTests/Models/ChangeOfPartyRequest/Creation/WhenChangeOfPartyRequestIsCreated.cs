using System;
using AutoFixture;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.UnitTests.Models.ChangeOfPartyRequest.Creation
{
    [TestFixture]
    public class WhenChangeOfPartyRequestIsCreated
    {
        private ChangeOfPartyRequestCreationTestFixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new ChangeOfPartyRequestCreationTestFixture();
        }

        [Test]
        public void ThenTheRequestHasCorrectApprenticeshipId()
        {
            _fixture.CreateChangeOfPartyRequest();
            Assert.AreEqual(_fixture.Apprenticeship.Id, _fixture.Result.ApprenticeshipId);
        }

        [Test]
        public void ThenTheRequestHasCorrectType()
        {
            _fixture.CreateChangeOfPartyRequest();
            Assert.AreEqual(_fixture.RequestType, _fixture.Result.ChangeOfPartyType);
        }

        [Test]
        public void ThenTheRequestHasCorrectOriginatingParty()
        {
            _fixture.CreateChangeOfPartyRequest();
            Assert.AreEqual(_fixture.OriginatingParty, _fixture.Result.OriginatingParty);
        }

        [Test]
        public void ThenTheRequestHasCorrectProviderId()
        {
            _fixture.CreateChangeOfPartyRequest();
            Assert.AreEqual(_fixture.ProviderId, _fixture.Result.ProviderId);
        }

        [Test]
        public void ThenTheRequestHasCorrectAccountLegalEntityId()
        {
            _fixture.CreateChangeOfPartyRequest();
            Assert.AreEqual(_fixture.AccountLegalEntityId, _fixture.Result.AccountLegalEntityId);
        }

        [Test]
        public void ThenTheRequestHasCorrectPrice()
        {
            _fixture.CreateChangeOfPartyRequest();
            Assert.AreEqual(_fixture.Price, _fixture.Result.Price);
        }

        [Test]
        public void ThenTheRequestHasCorrectStartDate()
        {
            _fixture.CreateChangeOfPartyRequest();
            Assert.AreEqual(_fixture.StartDate, _fixture.Result.StartDate);
        }

        [Test]
        public void ThenTheRequestHasCorrectEndDate()
        {
            _fixture.CreateChangeOfPartyRequest();
            Assert.AreEqual(_fixture.EndDate, _fixture.Result.EndDate);
        }

        [Test]
        public void ThenTheRequestStatusIsPending()
        {
            _fixture.CreateChangeOfPartyRequest();
            Assert.AreEqual(ChangeOfPartyRequestStatus.Pending, _fixture.Result.Status);
        }
    }

    internal class ChangeOfPartyRequestCreationTestFixture
    {
        public CommitmentsV2.Models.Apprenticeship Apprenticeship { get; private set; }
        public ChangeOfPartyRequestType RequestType { get; private set; }
        public Party OriginatingParty { get; private set; }
        public long? ProviderId { get; private set; }
        public long? AccountLegalEntityId { get; private set; }
        public int Price { get; private set; }
        public DateTime StartDate { get; private set; }
        public DateTime? EndDate { get; private set; }
        public CommitmentsV2.Models.ChangeOfPartyRequest Result { get; private set; }

        public ChangeOfPartyRequestCreationTestFixture()
        {
            var autoFixture = new Fixture();

            Apprenticeship = new CommitmentsV2.Models.Apprenticeship {Id = autoFixture.Create<long>()};
            RequestType = ChangeOfPartyRequestType.ChangeEmployer;
            OriginatingParty = Party.Provider;
            ProviderId = null;
            AccountLegalEntityId = autoFixture.Create<long>();
            Price = autoFixture.Create<int>();
            StartDate = autoFixture.Create<DateTime>();
            EndDate = autoFixture.Create<DateTime?>();
        }

        public ChangeOfPartyRequestCreationTestFixture WithRequestType(ChangeOfPartyRequestType requestType)
        {
            RequestType = requestType;
            return this;
        }

        public ChangeOfPartyRequestCreationTestFixture WithOriginatingParty(Party originatingParty)
        {
            OriginatingParty = originatingParty;
            return this;
        }

        public ChangeOfPartyRequestCreationTestFixture WithAccountLegalEntityId(long? accountLegalEntityId)
        {
            AccountLegalEntityId = accountLegalEntityId;
            return this;
        }

        public ChangeOfPartyRequestCreationTestFixture WithProviderId(long? providerId)
        {
            ProviderId = providerId;
            return this;
        }

        public ChangeOfPartyRequestCreationTestFixture WithPrice(int price)
        {
            Price = price;
            return this;
        }

        public ChangeOfPartyRequestCreationTestFixture WithStartDate(DateTime startDate)
        {
            StartDate = startDate;
            return this;
        }

        public ChangeOfPartyRequestCreationTestFixture WithEndDate(DateTime? endDate)
        {
            EndDate = endDate;
            return this;
        }

        public void CreateChangeOfPartyRequest()
        {
            Result = new CommitmentsV2.Models.ChangeOfPartyRequest(Apprenticeship, RequestType, OriginatingParty,
                AccountLegalEntityId, ProviderId, Price, StartDate, EndDate);
        }
    }
}
