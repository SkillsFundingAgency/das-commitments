using System.Threading.Tasks;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Application.Commands.AddCohort;
using SFA.DAS.CommitmentsV2.Mapping.RequestToCommandMappers;

namespace SFA.DAS.CommitmentsV2.UnitTests.Mapping.RequestToCommandMappers
{
    [TestFixture()]
    [Parallelizable(ParallelScope.Children)]
    public class CreateCohortWithOtherPartyRequestToAddCohortWithOtherPartyCommandMapperTests : MapperTester<CreateCohortWithOtherPartyRequestToAddCohortWithOtherPartyCommandMapper, CreateCohortWithOtherPartyRequest, AddCohortWithOtherPartyCommand>
    {
        [Test]
        public Task Map_AccountId_ShouldBeSet()
        {
            const long accountId = 123;
            return AssertPropertySet(input => input.AccountId = accountId, output => output.AccountId == accountId);
        }

        [Test]
        public Task Map_AccountLegalEntityId_ShouldBeSet()
        {
            const long accountLegalEntityId = 123;
            return AssertPropertySet(input => input.AccountLegalEntityId = accountLegalEntityId, output => output.AccountLegalEntityId == accountLegalEntityId);
        }

        [Test]
        public Task Map_ProviderId_ShouldBeSet()
        {
            const long providerId = 123;
            return AssertPropertySet(input => input.ProviderId = providerId, output => output.ProviderId == providerId);
        }

        [Test]
        public Task Map_Message_ShouldBeSet()
        {
            const string message = "hello";
            return AssertPropertySet(input => input.Message = message, output => output.Message == message);
        }
    }
}