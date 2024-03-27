using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Application.Commands.DeleteDraftApprenticeship;
using SFA.DAS.CommitmentsV2.Mapping.RequestToCommandMappers;

namespace SFA.DAS.CommitmentsV2.UnitTests.Mapping.RequestToCommandMappers
{
    [TestFixture]
    public class DeleteDraftApprenticeshipRequestToDeleteDraftApprenticeshipCommandMapperTests :
        OldMapperTester<DeleteDraftApprenticeshipRequestToDeleteDraftApprenticeshipCommandMapper,
        DeleteDraftApprenticeshipRequest,
        DeleteDraftApprenticeshipCommand>
    {
        [Test]
        public Task Map_UserId_ShouldBeSet()
        {
            const string userId = "123";
            return AssertPropertySet(input => input.UserInfo = new Types.UserInfo { UserId = userId }, output => output.UserInfo.UserId == userId);
        }

        [Test]
        public Task Map_DisplayName_ShouldBeSet()
        {
            const string displayName = "displayName";
            return AssertPropertySet(input => input.UserInfo = new Types.UserInfo { UserDisplayName = displayName }, output => output.UserInfo.UserDisplayName == displayName);
        }

        [Test]
        public Task Map_UserEmail_ShouldBeSet()
        {
            const string userEmail = "abc@test.com";
            return AssertPropertySet(input => input.UserInfo = new Types.UserInfo { UserEmail = userEmail }, output => output.UserInfo.UserEmail == userEmail);
        }
    }
}
