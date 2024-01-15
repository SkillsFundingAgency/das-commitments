using System;
using System.Linq;
using AutoFixture;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Domain.Exceptions;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.UnitOfWork.Context;

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
            Assert.That(_fixture.Result.ApprenticeshipId, Is.EqualTo(_fixture.Apprenticeship.Id));
        }

        [Test]
        public void ThenTheRequestHasCorrectType()
        {
            _fixture.CreateChangeOfPartyRequest();
            Assert.That(_fixture.Result.ChangeOfPartyType, Is.EqualTo(_fixture.RequestType));
        }

        [Test]
        public void ThenTheRequestHasCorrectOriginatingParty()
        {
            _fixture.CreateChangeOfPartyRequest();
            Assert.That(_fixture.Result.OriginatingParty, Is.EqualTo(_fixture.OriginatingParty));
        }

        [TestCase(Party.Employer)]
        [TestCase(Party.Provider)]
        public void ThenTheRequestHasCorrectNewPartyId(Party originatingParty)
        {
            _fixture
                .WithOriginatingParty(originatingParty)
                .WithValidRequestTypeForOriginatingParty(originatingParty)
                .CreateChangeOfPartyRequest();

            if (originatingParty == Party.Provider)
            {
                Assert.That(_fixture.Result.AccountLegalEntityId, Is.EqualTo(_fixture.NewPartyId));
                Assert.That(_fixture.Result.ProviderId, Is.EqualTo(null));
            }
            else
            {
                Assert.That(_fixture.Result.ProviderId, Is.EqualTo(_fixture.NewPartyId));
                Assert.That(_fixture.Result.AccountLegalEntityId, Is.EqualTo(null));
            }
        }

        [Test]
        public void ThenTheRequestHasCorrectPrice()
        {
            _fixture.CreateChangeOfPartyRequest();
            Assert.That(_fixture.Result.Price, Is.EqualTo(_fixture.Price));
        }

        [Test]
        public void ThenTheRequestHasCorrectStartDate()
        {
            _fixture.CreateChangeOfPartyRequest();
            Assert.That(_fixture.Result.StartDate, Is.EqualTo(_fixture.StartDate));
        }

        [Test]
        public void ThenTheRequestHasCorrectEndDate()
        {
            _fixture.CreateChangeOfPartyRequest();
            Assert.That(_fixture.Result.EndDate, Is.EqualTo(_fixture.EndDate));
        }

        [Test]
        public void ThenTheRequestHasCorrectEmploymentEndDate()
        {
            _fixture.CreateChangeOfPartyRequest();
            Assert.That(_fixture.Result.EmploymentEndDate, Is.EqualTo(_fixture.EmploymentEndDate));
        }

        [Test]
        public void ThenTheRequestHasCorrectEmploymentPrice()
        {
            _fixture.CreateChangeOfPartyRequest();
            Assert.That(_fixture.Result.EmploymentPrice, Is.EqualTo(_fixture.EmploymentPrice));
        }

        [Test]
        public void ThenTheRequestStatusIsPending()
        {
            _fixture.CreateChangeOfPartyRequest();
            Assert.That(_fixture.Result.Status, Is.EqualTo(ChangeOfPartyRequestStatus.Pending));
        }

        [Test]
        public void ThenTheRequestHasCorrectLastUpdatedOn()
        {
            _fixture.CreateChangeOfPartyRequest();
            Assert.That(_fixture.Result.LastUpdatedOn, Is.EqualTo(_fixture.Now));
        }

        [TestCase(Party.Provider, false)]
        [TestCase(Party.Employer, false)]
        [TestCase(Party.TransferSender, true)]
        [TestCase(Party.None, true)]
        public void ThenTheOriginatingPartyMustBeValid(Party originatingParty, bool expectThrow)
        {
            _fixture
                .WithOriginatingParty(originatingParty)
                .WithValidRequestTypeForOriginatingParty(originatingParty)
                .WithValidRequestTypeForOriginatingParty(originatingParty)
                .CreateChangeOfPartyRequest();

            if (expectThrow) _fixture.VerifyException<DomainException>();
        }

        [TestCase(Party.Provider, ChangeOfPartyRequestType.ChangeEmployer, false)]
        [TestCase(Party.Employer, ChangeOfPartyRequestType.ChangeProvider, false)]
        [TestCase(Party.Employer, ChangeOfPartyRequestType.ChangeEmployer, true)]
        [TestCase(Party.Provider, ChangeOfPartyRequestType.ChangeProvider, true)]
        public void ThenTheRequestTypeMustBeValid(Party originatingParty, ChangeOfPartyRequestType requestType,
            bool expectThrow)
        {
            _fixture
                .WithOriginatingParty(originatingParty)
                .WithRequestType(requestType)
                .CreateChangeOfPartyRequest();

            if (expectThrow) _fixture.VerifyException<DomainException>();
        }

        [TestCase(Party.Provider)]
        [TestCase(Party.Employer)]
        public void ThenTheStateChangesAreTracked(Party originatingParty)
        {
            _fixture
                .WithOriginatingParty(originatingParty)
                .WithValidRequestTypeForOriginatingParty(originatingParty)
                .CreateChangeOfPartyRequest();

            _fixture.VerifyTracking();
        }

        [TestCase(Party.Provider)]
        [TestCase(Party.Employer)]
        public void ThenAChangeOfPartyRequestCreatedEventIsEmitted(Party originatingParty)
        {
            _fixture
                .WithOriginatingParty(originatingParty)
                .WithValidRequestTypeForOriginatingParty(originatingParty)
                .CreateChangeOfPartyRequest();

            _fixture.VerifyEvent();
        }

        [TestCase(-100, true)]
        [TestCase(0, true)]
        [TestCase(100001, true)]
        [TestCase(100000, false)]
        [TestCase(1, false)]
        public void ThenPriceMustBeValid(int? price, bool expectThrow)
        {
            _fixture
                .WithPrice(price)
                .CreateChangeOfPartyRequest();

            _fixture.VerifyException<DomainException>(expectThrow);
        }
    }

    internal class ChangeOfPartyRequestCreationTestFixture
    {
        public CommitmentsV2.Models.Apprenticeship Apprenticeship { get; private set; }
        public ChangeOfPartyRequestType RequestType { get; private set; }
        public Party OriginatingParty { get; private set; }
        public long NewPartyId { get; private set; }
        public int? Price { get; private set; }
        public DateTime? StartDate { get; private set; }
        public DateTime? EndDate { get; private set; }
        public DateTime? EmploymentEndDate { get; private set; }
        public int? EmploymentPrice { get; private set; }
        public DeliveryModel? DeliveryModel { get; private set; }
        public UserInfo UserInfo { get; private set; }
        public CommitmentsV2.Models.ChangeOfPartyRequest Result { get; private set; }
        public Exception Exception { get; private set; }
        public UnitOfWorkContext UnitOfWorkContext { get; private set; }
        public DateTime Now { get; private set; }
        public bool HasOverlappingTrainingDates { get; set; }

        public ChangeOfPartyRequestCreationTestFixture()
        {
            var autoFixture = new Fixture();

            UnitOfWorkContext = new UnitOfWorkContext();

            Now = DateTime.UtcNow;

            Apprenticeship = new CommitmentsV2.Models.Apprenticeship
            {
                Id = autoFixture.Create<long>(),
                Cohort = new CommitmentsV2.Models.Cohort
                {
                    EmployerAccountId = autoFixture.Create<long>(),
                    ProviderId = autoFixture.Create<long>()
                }
            };

            RequestType = ChangeOfPartyRequestType.ChangeEmployer;
            OriginatingParty = Party.Provider;
            NewPartyId = autoFixture.Create<long>();
            Price = autoFixture.Create<int>();
            StartDate = autoFixture.Create<DateTime>();
            EndDate = autoFixture.Create<DateTime?>();
            EmploymentEndDate = autoFixture.Create<DateTime?>();
            EmploymentPrice = autoFixture.Create<int?>();
            UserInfo = new UserInfo();
        }

        public ChangeOfPartyRequestCreationTestFixture WithOriginatingParty(Party originatingParty)
        {
            OriginatingParty = originatingParty;
            return this;
        }

        public ChangeOfPartyRequestCreationTestFixture WithRequestType(ChangeOfPartyRequestType requestType)
        {
            RequestType = requestType;
            return this;
        }

        public ChangeOfPartyRequestCreationTestFixture WithValidRequestTypeForOriginatingParty(Party originatingParty)
        {
            RequestType = originatingParty == Party.Provider
                ? ChangeOfPartyRequestType.ChangeEmployer
                : ChangeOfPartyRequestType.ChangeProvider;
            return this;
        }

        public ChangeOfPartyRequestCreationTestFixture WithNewPartyId(long newPartyId)
        {
            NewPartyId = newPartyId;
            return this;
        }

        public ChangeOfPartyRequestCreationTestFixture WithPrice(int? price)
        {
            Price = price;
            return this;
        }

        public ChangeOfPartyRequestCreationTestFixture WithStartDate(DateTime? startDate)
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
            Exception = null;

            try
            {
                Result = new CommitmentsV2.Models.ChangeOfPartyRequest(
                    Apprenticeship,
                    RequestType,
                    OriginatingParty,
                    NewPartyId,
                    Price,
                    StartDate,
                    EndDate,
                    EmploymentPrice,
                    EmploymentEndDate,
                    DeliveryModel,
                    HasOverlappingTrainingDates,
                    UserInfo,
                    Now);
            }
            catch (Exception ex)
            {
                Exception = ex;
            }
        }

        public void VerifyException<T>(bool isThrown = true)
        {
            if (isThrown)
            {
                Assert.That(Exception, Is.Not.Null);
                Assert.That(Exception, Is.InstanceOf<T>());
            }
            else
            {
                Assert.That(Exception, Is.Null);
            }
        }

        public void VerifyTracking()
        {
            Assert.That(UnitOfWorkContext.GetEvents().SingleOrDefault(x => x is EntityStateChangedEvent @event
                                                                                && @event.EntityType ==
                                                                                nameof(ChangeOfPartyRequest)), Is.Not.Null);
        }

        public void VerifyEvent()
        {
            Assert.That(UnitOfWorkContext.GetEvents().SingleOrDefault(x => x is ChangeOfPartyRequestCreatedEvent), Is.Not.Null);
        }
    }
}
