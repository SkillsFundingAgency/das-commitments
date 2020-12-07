using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.Commitments.Domain.Api.Requests;
using SFA.DAS.Commitments.Domain.Api.Types;
using SFA.DAS.Commitments.Domain.Interfaces;
using SFA.DAS.Commitments.Infrastructure.Api.Requests;
using SFA.DAS.NLog.Logger;

namespace SFA.DAS.Commitments.Infrastructure.UnitTests.Services.AssessmentOrgsService
{
    [TestFixture]
    public class WhenCallingAssessmentOrgsService
    {
        private Infrastructure.Services.AssessmentOrgsService _sut;

        private Mock<IApiClient> _assessmentOrgsApi;

        [SetUp]
        public void SetUp()
        {
            _assessmentOrgsApi = new Mock<IApiClient>();
            _sut = new Infrastructure.Services.AssessmentOrgsService(_assessmentOrgsApi.Object, Mock.Of<ILog>());
        }

        [Test]
        public void WhenGettingExceptionsFromApi()
        {
            _assessmentOrgsApi.Setup(m => m.Get<EpaoResponse>(It.IsAny<GetEpaoOrganisationsRequest>())).Throws<Exception>();

            Func<Task<IEnumerable<OrganisationSummary>>> act = async () => await _sut.All();

            act.ShouldThrow<Exception>();

            _assessmentOrgsApi.Verify(m => m.Get<EpaoResponse>(It.IsAny<GetEpaoOrganisationsRequest>()), Times.Exactly(4));
        }

        [Test]
        public async Task WhenCallingPaymentService()
        {
            _assessmentOrgsApi.Setup(m => m.Get<EpaoResponse>(It.IsAny<GetEpaoOrganisationsRequest>()))
                .ReturnsAsync(new EpaoResponse{Epaos = new List<OrganisationSummary>()});

            await _sut.All();

            _assessmentOrgsApi.Verify(m => m.Get<EpaoResponse>(It.IsAny<GetEpaoOrganisationsRequest>()), Times.Once);
        }
    }
}