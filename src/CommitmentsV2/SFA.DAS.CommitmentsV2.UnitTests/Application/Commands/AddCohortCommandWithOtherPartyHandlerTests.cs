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
    public class AddCohortCommandWithOtherPartyHandlerTests
    { 

        private AddCohortCommandWithOtherPartyHandlerTestsFixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new AddCohortCommandWithOtherPartyHandlerTestsFixture();
        }

        [TearDown]
        public void TearDown()
        {
            _fixture?.Dispose();
        }

        [Test]
        public async Task Handle_WhenHandled_ShouldCallCreateCohortWithOtherParty()
        {
            const long providerId = 1;
            const long accountId = 2;
            const long accountLegalEntityId = 3;
            long? transferSenderId = 4;
            int? pledgeApplicationId = 5;
            const string message = "Message";

            await _fixture.Handle(accountId, accountLegalEntityId, providerId, transferSenderId, pledgeApplicationId, message);

            _fixture.CohortDomainServiceMock.Verify(x => x.CreateCohortWithOtherParty(providerId, accountId, accountLegalEntityId, transferSenderId, pledgeApplicationId, message, _fixture.UserInfo, It.IsAny<CancellationToken>()));
        }


        [Test]
        public async Task Handle_WhenHandled_ShouldReturnCreateResponseWithCorrectReferenceAndCohortId()
        {
            const string expectedHash = "ABC123";

            _fixture.WithGeneratedHash(expectedHash);

            var response = await _fixture.Handle(1,123, 2323, null, null,"Message1");

            Assert.That(response.Reference, Is.EqualTo(expectedHash));
            Assert.That(response.Id, Is.EqualTo(_fixture.CohortId));
        }
    }

    public class AddCohortCommandWithOtherPartyHandlerTestsFixture : IDisposable
    {
        public ProviderCommitmentsDbContext Db { get; set; }
        public long CohortId { get; set; }
        public Cohort Cohort { get; set; }

        public AddCohortCommandWithOtherPartyHandlerTestsFixture()
        {
            Db = new ProviderCommitmentsDbContext(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>()
                                                    .UseInMemoryDatabase(Guid.NewGuid().ToString(), b => b.EnableNullChecks(false))
                                                    .Options);
            CohortId = 8768;
            Cohort = new Cohort{Id = CohortId};

            EncodingServiceMock = new Mock<IEncodingService>();

            CohortDomainServiceMock = new Mock<ICohortDomainService>();
            CohortDomainServiceMock.Setup(x => x.CreateCohortWithOtherParty(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<long>(), It.IsAny<long?>(), 
                    It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<UserInfo>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Cohort);

            UserInfo = new UserInfo();
        }

        public Mock<IEncodingService> EncodingServiceMock { get; }
        public IEncodingService EncodingService => EncodingServiceMock.Object;
        public Mock<ICohortDomainService> CohortDomainServiceMock { get; }
        public UserInfo UserInfo { get; }

        public AddCohortCommandWithOtherPartyHandlerTestsFixture WithGeneratedHash(string hash)
        {
            EncodingServiceMock
                .Setup(hs => hs.Encode(It.IsAny<long>(), It.Is<EncodingType>(encoding => encoding == EncodingType.CohortReference)))
                .Returns(hash);

            return this;
        }

        public async Task<AddCohortResult> Handle(long accountId, long accountLegalEntity, long providerId, long? transferSenderId, int? pledgeApplicationId, string message)
        {
            Db.SaveChanges();
            
            var command = new AddCohortWithOtherPartyCommand(accountId, accountLegalEntity, providerId, transferSenderId, pledgeApplicationId, message, UserInfo);

            var handler = new AddCohortWithOtherPartyHandler(new Lazy<ProviderCommitmentsDbContext>(() => Db),
                EncodingService,
                Mock.Of<ILogger<AddCohortWithOtherPartyHandler>>(),
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