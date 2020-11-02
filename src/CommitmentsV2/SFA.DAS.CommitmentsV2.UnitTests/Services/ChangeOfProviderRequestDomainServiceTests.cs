using System.Threading.Tasks;
using NUnit.Framework;
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
            await _fixture.CreateChangeOfPartyRequest();
            _fixture.VerifyAggregateMethodInvoked();
        }

        [Test]
        public async Task CreateChangeOfPartyRequest_Returns_Result_From_Aggregate()
        {
            await _fixture.CreateChangeOfPartyRequest();
            _fixture.VerifyResult();
        }

        [Test]
        public async Task CreateChangeOfPartyRequest_Saves_Request_To_DbContext()
        {
            await _fixture.CreateChangeOfPartyRequest();
            _fixture.VerifyResultAddedToDbContext();
        }

        // TODO : is there any restrictions from employer side they cant change provider
        //[Test]
        //public async Task CreateChangeOfPartyRequest_Throws_If_Provider_Does_Not_Have_Permission()
        //{
        //    _fixture.WithNoProviderPermission();
        //    await _fixture.CreateChangeOfPartyRequest();
        //    _fixture.VerifyException<DomainException>();
        //}
    }
}
