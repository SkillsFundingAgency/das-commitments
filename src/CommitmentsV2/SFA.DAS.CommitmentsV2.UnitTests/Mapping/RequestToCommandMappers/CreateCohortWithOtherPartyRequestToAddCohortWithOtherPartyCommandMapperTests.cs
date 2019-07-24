using System;
using System.Threading.Tasks;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Application.Commands.AddCohort;
using SFA.DAS.CommitmentsV2.Mapping.RequestToCommandMappers;

namespace SFA.DAS.CommitmentsV2.UnitTests.Mapping.RequestToCommandMappers
{
    [TestFixture()]
    public class CreateCohortWithOtherPartyRequestToAddCohortWithOtherPartyCommandMapperTests : MapperTester<CreateCohortWithOtherPartyRequestToAddCohortWithOtherPartyCommandMapper, CreateCohortWithOtherPartyRequest, AddCohortWithOtherPartyCommand>
    {
        [Test]
        public Task Map_AccountLegalEntityId_ShouldBeSet()
        {
            const long accountLegalEntityIdId = 123;
            return AssertPropertySet(input => input.AccountLegalEntityId = accountLegalEntityIdId, output => output.AccountLegalEntityId == accountLegalEntityIdId);
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
