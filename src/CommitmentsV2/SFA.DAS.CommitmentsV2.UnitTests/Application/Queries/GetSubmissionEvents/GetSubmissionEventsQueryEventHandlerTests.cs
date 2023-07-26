using AutoFixture;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Application.Queries.GetSubmissionEvents;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Models.ApprovalsOuterApi;
using SFA.DAS.CommitmentsV2.Models.ApprovalsOuterApi.Types;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Queries.GetSubmissionEvents
{
    [TestFixture]
    public class GetSubmissionEventsQueryEventHandlerTests
    {
        [Test]
        public async Task Submission_OuterApi_ToGet_SubmissionEvent_IsCalled()
        {
            var fixture = new GetSubmissionEventsQueryEventHandlerTestsFixture();
            await fixture.Handle();

            fixture.VerifyOuterApiIsCalled();
        }

        [Test]
        public async Task Submission_Verify_SubmissionEvents_Are_Returned()
        {
            var fixture = new GetSubmissionEventsQueryEventHandlerTestsFixture();
            await fixture.Handle();

            fixture.VerifySubmissionEventsAreReturned();
        }

        public class GetSubmissionEventsQueryEventHandlerTestsFixture
        {
            private GetSubmissionEventQueryHandler _sut { get; set; }

            private GetSubmissionEventsQuery _query;
            private Mock<IApprovalsOuterApiClient> _approvalOuterApi;
            private PageOfResults<SubmissionEvent> _expectedResult;
            private PageOfResults<SubmissionEvent> _result;

            public GetSubmissionEventsQueryEventHandlerTestsFixture()
            {
                 var autoFixture = new AutoFixture.Fixture();
                _expectedResult = autoFixture.Create<PageOfResults<SubmissionEvent>>();
                _query = autoFixture.Create<GetSubmissionEventsQuery>();

                _approvalOuterApi = new Mock<IApprovalsOuterApiClient>();
                _approvalOuterApi.Setup(x => x.Get<PageOfResults<SubmissionEvent>>(It.IsAny<GetSubmissionsEventsRequest>())).ReturnsAsync(_expectedResult);
                _sut = new GetSubmissionEventQueryHandler(_approvalOuterApi.Object);
            }

            public async Task Handle()
            {
                _result = await _sut.Handle(_query, CancellationToken.None);
            }

            internal void VerifySubmissionEventsAreReturned()
            {
                Assert.AreEqual(_expectedResult, _result);
            }

            internal void VerifyOuterApiIsCalled()
            {
                _approvalOuterApi.Verify(x => 
                x.Get<PageOfResults<SubmissionEvent>>(It.Is<GetSubmissionsEventsRequest>(y => y.SinceEventId == _query.LastSubmissionEventId)), Times.Once);
            }
        }
    }
}
