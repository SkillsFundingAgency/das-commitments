using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.Apprenticeships.Api.Types.AssessmentOrgs;
using SFA.DAS.AssessmentOrgs.Api.Client;
using SFA.DAS.NLog.Logger;

namespace SFA.DAS.Commitments.Infrastructure.UnitTests.Services.AssessmentOrgsService
{
    [TestFixture]
    public class WhenCallingAssessmentOrgsService
    {
        private Infrastructure.Services.AssessmentOrgsService _sut;

        private Mock<IAssessmentOrgsApiClient> _assessmentOrgsApi;

        [SetUp]
        public void SetUp()
        {
            _assessmentOrgsApi = new Mock<IAssessmentOrgsApiClient>();
            _sut = new Infrastructure.Services.AssessmentOrgsService(_assessmentOrgsApi.Object, Mock.Of<ILog>());
        }

        [Test]
        public void WhenGettingExceptionsFromApi()
        {
            _assessmentOrgsApi.Setup(m => m.FindAllAsync()).Throws<Exception>();

            Func<Task<IEnumerable<OrganisationSummary>>> act = async () => await _sut.All();

            act.ShouldThrow<Exception>();

            _assessmentOrgsApi.Verify(m => m.FindAllAsync(), Times.Exactly(4));
        }

        [Test]
        public async Task WhenCallingPaymentService()
        {
            _assessmentOrgsApi.Setup(m => m.FindAllAsync())
                .ReturnsAsync(new OrganisationSummary[0]);

            await _sut.All();

            _assessmentOrgsApi.Verify(m => m.FindAllAsync(), Times.Once);
        }
    }
}