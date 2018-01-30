using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Ploeh.AutoFixture;
using SFA.DAS.Commitments.Application.Queries.GetApprenticeships;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;

namespace SFA.DAS.Commitments.Application.UnitTests.Queries.GetApprenticeships
{
    [TestFixture]
    public sealed class WhenGettingApprenticeships
    {
        private Mock<IApprenticeshipRepository> _mockApprenticeshipRespository;
        private GetApprenticeshipsQueryHandler _handler;
        private Apprenticeship _apprenticeship;

        [SetUp]
        public void SetUp()
        {
            _mockApprenticeshipRespository = new Mock<IApprenticeshipRepository>();
            _handler = new GetApprenticeshipsQueryHandler(_mockApprenticeshipRespository.Object);

            var dataFixture = new Fixture();
            _apprenticeship = dataFixture.Build<Apprenticeship>().Create();
        }

        [Test]
        public async Task ThenTheCommitmentRepositoryIsCalledForTheEmployer()
        {
            _mockApprenticeshipRespository.Setup(x => x.GetActiveApprenticeshipsByEmployer(It.IsAny<long>()))
                .ReturnsAsync(new ApprenticeshipsResult());

            await _handler.Handle(new GetApprenticeshipsRequest
            {
                Caller = new Caller
                {
                    CallerType = CallerType.Employer,
                    Id = _apprenticeship.EmployerAccountId
                }
            });

            _mockApprenticeshipRespository.Verify(x => x.GetActiveApprenticeshipsByEmployer(
                It.Is<long>(id => id == _apprenticeship.EmployerAccountId)), Times.Once);
        }

        [Test]
        public async Task ThenTheCommitmentRepositoryIsCalledForTheProvider()
        {
            _mockApprenticeshipRespository.Setup(x => x.GetActiveApprenticeshipsByProvider(It.IsAny<long>()))
                .ReturnsAsync(new ApprenticeshipsResult());

            await _handler.Handle(new GetApprenticeshipsRequest
            {
                Caller = new Caller
                {
                    CallerType = CallerType.Provider,
                    Id = _apprenticeship.ProviderId
                }
            });

            _mockApprenticeshipRespository.Verify(x => x.GetActiveApprenticeshipsByProvider(
                It.Is<long>(id => id == _apprenticeship.ProviderId)), Times.Once);
        }
    }
}
