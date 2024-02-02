using System.Threading.Tasks;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Domain.Exceptions;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.UnitTests.Services
{
    [TestFixture]
    public class ChangeOfEmployerRequestDomainServiceTests
    {
        private ChangeOfPartyRequestDomainServiceTestsFixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new ChangeOfPartyRequestDomainServiceTestsFixture(Party.Provider, ChangeOfPartyRequestType.ChangeEmployer);
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
        public async Task CreateChangeOfPartyRequest_Throws_If_Party_Is_Not_Provider()
        {

            //Act
            _fixture.WithOriginatingParty(Party.Employer);
            await _fixture.CreateChangeOfPartyRequest();

            //Assert
            _fixture.VerifyException<DomainException>();
        }

        [Test]
        public async Task CreateChangeOfPartyRequest_Throws_If_Provider_Does_Not_Have_Permission()
        {
            //Act
            _fixture.WithNoProviderPermission();
            await _fixture.CreateChangeOfPartyRequest();

            //Assert
            _fixture.VerifyException<DomainException>();
        }

        [Test]
        public async Task CreateChangeOfPartyRequest_Throws_exception_if_current_provider_is_chosen_by_employer_during_a_change_of_provider_request()
        {
            _fixture.WithOriginatingParty(Party.Employer);
            _fixture.WithSameTrainingProviderWhenRequestingChangeOfProvider();

            await _fixture.CreateChangeOfPartyRequest();

            _fixture.VerifyException<DomainException>();
        }

        [Test]
        public async Task CreateChangeOfPartyRequest_Throws_exception_if_apprenticeship_is_FlexiJob_during_a_change_of_provider_request()
        {
            _fixture.WithOriginatingParty(Party.Employer);
            _fixture.WithDeliveryModelAsFlexiJobAndChangeOfProvider();

            await _fixture.CreateChangeOfPartyRequest();

            _fixture.VerifyException<DomainException>();
        }


    }
}
