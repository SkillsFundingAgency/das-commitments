using SFA.DAS.CommitmentsV2.Domain.Exceptions;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.UnitTests.Services
{
    [TestFixture]
    public class ChangeOfProviderRequestDomainServiceTests
    {
        private ChangeOfPartyRequestDomainServiceTestsFixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new ChangeOfPartyRequestDomainServiceTestsFixture(Party.Employer, ChangeOfPartyRequestType.ChangeProvider);
        }

        [Test]
        public async Task CreateChangeOfPartyRequest_Invokes_Aggregate_State_Change()
        {
            //Act
            await _fixture.CreateChangeOfPartyRequest();

            //Assert
            _fixture.VerifyAggregateMethodInvoked();
        }

        [Test]
        public async Task CreateChangeOfPartyRequest_Returns_Result_From_Aggregate()
        {
            //Act
            await _fixture.CreateChangeOfPartyRequest();

            //Assert
            _fixture.VerifyResult();
        }

        [Test]
        public async Task CreateChangeOfPartyRequest_Saves_Request_To_DbContext()
        {
            //Act
            await _fixture.CreateChangeOfPartyRequest();

            //Assert
            _fixture.VerifyResultAddedToDbContext();
        }

        [Test]
        public async Task CreateChangeOfPartyRequest_Throws_If_Party_Is_Not_Employer()
        {
            _fixture.WithOriginatingParty(Party.Provider);

            //Act
            await _fixture.CreateChangeOfPartyRequest();

            //Assert
            _fixture.VerifyException<DomainException>();
        }


        [TestCase(false, false, true)]
        [TestCase(true, false, true)]
        [TestCase(false, true, false)]
        [TestCase(true, true, false)]

        public async Task ValidateChangeOfEmployerOverlap(bool hasOverlappingStartDate, bool hasOverlappingEndDate, bool expectedValidationOutcome)
        {
            _fixture.WithOverlapCheckResult(hasOverlappingStartDate, hasOverlappingEndDate);

            //Act
            await _fixture.ValidateChangeOfEmployerOverlap();

            //Assert
            if (expectedValidationOutcome)
            {
                _fixture.VerifyNotException<DomainException>();

            }
            else
            {
                _fixture.VerifyException<DomainException>();
            }
        }
    }
}
