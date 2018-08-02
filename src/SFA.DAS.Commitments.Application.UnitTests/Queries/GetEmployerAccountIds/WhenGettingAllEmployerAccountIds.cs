using System.Collections.Generic;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.Commitments.Application.Queries.GetEmployerAccountIds;
using SFA.DAS.Commitments.Domain.Data;

namespace SFA.DAS.Commitments.Application.UnitTests.Queries.GetEmployerAccountIds
{
    [TestFixture]
    public class WhenGettingAllEmployerAccountIds
    {
        private Mock<IApprenticeshipRepository> _mockRepository;
        private GetEmployerAccountIdsQueryHandler _handler;
        private List<long> _fakeEmployerAccountIds;

        [SetUp]
        public void SetUp()
        {
            _mockRepository = new Mock<IApprenticeshipRepository>();
            _handler = new GetEmployerAccountIdsQueryHandler(_mockRepository.Object);

            var dataFixture = new Fixture();
            _fakeEmployerAccountIds = dataFixture.Build<List<long>>().Create();
            _mockRepository.Setup(x => x.GetEmployerAccountIds()).ReturnsAsync(_fakeEmployerAccountIds);
        }

        [Test]
        public async Task ThenTheEmployerIdsReturnedShouldMatchTheExpectedList()
        {
            var response = await _handler.Handle(new GetEmployerAccountIdsRequest());

            response.Data.Should().BeSameAs(_fakeEmployerAccountIds);
        }
    }
}
