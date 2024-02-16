using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Application.Commands.AddCohort;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.Encoding;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Commands
{
    [TestFixture]
    [Parallelizable]
    public class CreateEmptyCohortHandlerTests
    { 
        private CreateEmptyCohortHandlerTestsFixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new CreateEmptyCohortHandlerTestsFixture();
        }
        
        [TearDown]
        public void TearDown()
        {
            _fixture?.Dispose();
        }

        [Test]
        public async Task Handle_WhenHandled_ShouldCreateAnEmptyCohort()
        {
            const long providerId = 1;
            const long accountId = 2;
            const long accountLegalEntityId = 3;

            await _fixture.Handle(accountId, accountLegalEntityId, providerId);

            _fixture.CohortDomainServiceMock.Verify(x => x.CreateEmptyCohort(providerId, accountId, accountLegalEntityId, _fixture.UserInfo, It.IsAny<CancellationToken>()));
        }

        [Test]
        public async Task Handle_WhenHandled_ShouldReturnCreateResponseWithCorrectReferenceAndCohortId()
        {
            const string expectedHash = "ABC123";

            _fixture.WithGeneratedHash(expectedHash);

            var response = await _fixture.Handle(1,123, 2323);

            Assert.Multiple(() =>
            {
                Assert.That(response.Reference, Is.EqualTo(expectedHash));
                Assert.That(response.Id, Is.EqualTo(_fixture.CohortId));
            });
        }
    }

    public class CreateEmptyCohortHandlerTestsFixture : IDisposable
    {
        public ProviderCommitmentsDbContext Db { get; set; }
        public long CohortId { get; set; }
        public Cohort Cohort { get; set; }

        public CreateEmptyCohortHandlerTestsFixture()
        {
            Db = new ProviderCommitmentsDbContext(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>()
                                                    .UseInMemoryDatabase(Guid.NewGuid().ToString(), b => b.EnableNullChecks(false))
                                                    .Options);
            CohortId = 8768;
            Cohort = new Cohort{Id = CohortId};

            EncodingServiceMock = new Mock<IEncodingService>();

            CohortDomainServiceMock = new Mock<ICohortDomainService>();
            CohortDomainServiceMock.Setup(x => x.CreateEmptyCohort(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<long>(), It.IsAny<UserInfo>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Cohort);

            UserInfo = new UserInfo();
        }

        public Mock<IEncodingService> EncodingServiceMock { get; }
        public IEncodingService EncodingService => EncodingServiceMock.Object;
        public Mock<ICohortDomainService> CohortDomainServiceMock { get; }
        public UserInfo UserInfo { get; }

        public CreateEmptyCohortHandlerTestsFixture WithGeneratedHash(string hash)
        {
            EncodingServiceMock
                .Setup(hs => hs.Encode(It.IsAny<long>(), It.Is<EncodingType>(encoding => encoding == EncodingType.CohortReference)))
                .Returns(hash);

            return this;
        }

        public async Task<AddCohortResult> Handle(long accountId, long accountLegalEntity, long providerId)
        {
            Db.SaveChanges();
            
            var command = new AddEmptyCohortCommand(accountId, accountLegalEntity, providerId, UserInfo);

            var handler = new AddEmptyCohortHandler(new Lazy<ProviderCommitmentsDbContext>(() => Db),
                EncodingService,
                Mock.Of<ILogger<AddEmptyCohortHandler>>(),
                CohortDomainServiceMock.Object);

            var response = await handler.Handle(command, CancellationToken.None);
            await Db.SaveChangesAsync();

            return response;
        }

        public void Dispose()
        {
            Db?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}