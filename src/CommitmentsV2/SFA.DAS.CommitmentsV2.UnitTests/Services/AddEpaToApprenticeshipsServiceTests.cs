using System;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Application.Commands.AddLastSubmissionEventId;
using SFA.DAS.CommitmentsV2.Application.Commands.UpdateApprenticeshipsWithEpaOrgId;
using SFA.DAS.CommitmentsV2.Application.Commands.UpdateCacheOfAssessmentOrganisations;
using SFA.DAS.CommitmentsV2.Application.Queries.GetLastSubmissionEventId;
using SFA.DAS.CommitmentsV2.Application.Queries.GetSubmissionEvents;
using SFA.DAS.CommitmentsV2.Models.ApprovalsOuterApi.Types;
using SFA.DAS.CommitmentsV2.Services;

namespace SFA.DAS.CommitmentsV2.UnitTests.Services
{
    [TestFixture]
    public class AddEpaToApprenticeshipsServiceTests
    {
        [Test]
        public async Task GetLastSubmissionEventIdQuery_IsCalled()
        {
            var fixture = new AddEpaToApprenticeshipsServiceTestFixture();
            await fixture.Update();
            fixture.Verify_GetLastSubmissionEventIdQuery_IsCalled();
        }

        [Test]
        public async Task UpdateCacheOfAssessmentOrganisationsCommand_IsCalled()
        {
            var fixture = new AddEpaToApprenticeshipsServiceTestFixture();
            await fixture.Update();
            fixture.Verify_UpdateCacheOfAssessmentOrganisationsCommand_IsCalled();
        }

        [Test]
        public async Task GetSubmissionEventsQuery_IsCalled()
        {
            var fixture = new AddEpaToApprenticeshipsServiceTestFixture();
            await fixture.Update();
            fixture.Verify_GetSubmissionEventsQuery_IsCalled();
        }

        [Test]
        public async Task UpdateApprenticeshipsWithEpaOrgIdCommand_IsCalled()
        {
            var fixture = new AddEpaToApprenticeshipsServiceTestFixture();
            await fixture.Update();
            fixture.Verify_UpdateApprenticeshipsWithEpaOrgIdCommand_IsCalled();
        }

        [Test]
        public async Task AddLastSubmissionEventIdCommand_IsCalled()
        {
            var fixture = new AddEpaToApprenticeshipsServiceTestFixture();
            await fixture.Update();
            fixture.Verify_AddLastSubmissionEventIdCommand_IsCalled();
        }
  
        public class AddEpaToApprenticeshipsServiceTestFixture
        {
            AddEpaToApprenticeshipsService _sut;
            private PageOfResults<SubmissionEvent> _pageOfResult;
            Mock<IMediator> _mediator;
            readonly long _lastSubmissionEventId = 0;
            long? _pageLastId;

            public AddEpaToApprenticeshipsServiceTestFixture()
            {
                var fixture = new Fixture();
                _pageOfResult = fixture.Create<PageOfResults<SubmissionEvent>>();
                _pageLastId = fixture.Create<long>();
                _mediator = new Mock<IMediator>();
                _mediator.Setup(x => x.Send(It.IsAny<GetLastSubmissionEventIdQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(_lastSubmissionEventId);
                _mediator.Setup(x => x.Send(It.IsAny<UpdateCacheOfAssessmentOrganisationsCommand>(), It.IsAny<CancellationToken>()));
                _mediator.Setup(x => x.Send(It.Is<GetSubmissionEventsQuery>(x => x.LastSubmissionEventId == _lastSubmissionEventId), It.IsAny<CancellationToken>())).ReturnsAsync(_pageOfResult);
                _mediator.Setup(x => x.Send(It.IsAny<UpdateApprenticeshipsWithEpaOrgIdCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(_pageLastId);
                _mediator.Setup(x => x.Send(It.IsAny<AddLastSubmissionEventIdCommand>(), It.IsAny<CancellationToken>()));

                _sut = new AddEpaToApprenticeshipsService(_mediator.Object, Mock.Of<ILogger<AddEpaToApprenticeshipsService>>());
            }

            public async Task Update()
            {
                await _sut.Update();
            }

            internal void Verify_AddLastSubmissionEventIdCommand_IsCalled()
            {
                _mediator.Verify(x => x.Send(It.IsAny<AddLastSubmissionEventIdCommand>(), It.IsAny<CancellationToken>()), Times.Once());
            }

            internal void Verify_GetLastSubmissionEventIdQuery_IsCalled()
            {
                _mediator.Verify(x => x.Send(It.IsAny<GetLastSubmissionEventIdQuery>(), It.IsAny<CancellationToken>()), Times.Once());
            }

            internal void Verify_GetSubmissionEventsQuery_IsCalled()
            {
                _mediator.Verify(x => x.Send(It.Is<GetSubmissionEventsQuery>(x => x.LastSubmissionEventId == _lastSubmissionEventId), It.IsAny<CancellationToken>()), Times.Once());
            }

            internal void Verify_UpdateApprenticeshipsWithEpaOrgIdCommand_IsCalled()
            {
                _mediator.Verify(x => x.Send(It.Is<UpdateApprenticeshipsWithEpaOrgIdCommand>(x => x.SubmissionEvents == _pageOfResult.Items), It.IsAny<CancellationToken>()), Times.Once());
            }

            internal void Verify_UpdateCacheOfAssessmentOrganisationsCommand_IsCalled()
            {
                _mediator.Verify(x => x.Send(It.IsAny<UpdateCacheOfAssessmentOrganisationsCommand>(), It.IsAny<CancellationToken>()), Times.Once());
            }
        }
    }
}
