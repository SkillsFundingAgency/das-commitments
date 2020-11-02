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

        // TODO : Commenting this Test for the new feature Change of Provider
        //[Test(Description = "Temporary invariant disallowing Employer access to this feature")]
        //public async Task CreateChangeOfPartyRequest_Throws_If_Party_Is_Not_Provider()
        //{
        //    _fixture.WithOriginatingParty(Party.Employer);
        //    await _fixture.CreateChangeOfPartyRequest();
        //    _fixture.VerifyException<DomainException>();
        //}

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

        [Test]
        public async Task CreateChangeOfPartyRequest_Throws_If_Provider_Does_Not_Have_Permission()
        {
            _fixture.WithNoProviderPermission();
            await _fixture.CreateChangeOfPartyRequest();
            _fixture.VerifyException<DomainException>();
        }        
    }
}
