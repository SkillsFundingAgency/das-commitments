using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Application.Commands.AddCohort;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.Encoding;
using SFA.DAS.Testing;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Commands
{
    [TestFixture]
    [Parallelizable]
    public class AddCohortCommandWithOtherPartyHandlerTests : FluentTest<AddCohortCommandWithOtherPartyHandlerTestsFixture>
    { 

        private AddCohortCommandWithOtherPartyHandlerTestsFixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new AddCohortCommandWithOtherPartyHandlerTestsFixture();
        }

        [Test]
        public async Task Handle_WhenHandled_ShouldCallCreateCohortWithOtherParty()
        {
            const long providerId = 1;
            const long accountLegalEntityId = 3;
            const string message = "Message";

            await _fixture.Handle(accountLegalEntityId, providerId, message);

            _fixture.CohortDomainServiceMock.Verify(x => x.CreateCohortWithOtherParty(providerId, accountLegalEntityId, message, _fixture.UserInfo, It.IsAny<CancellationToken>()));
        }


        [Test]
        public async Task Handle_WhenHandled_ShouldReturnCreateResponseWithCorrectReferenceAndCohortId()
        {
            const string expectedHash = "ABC123";

            _fixture.WithGeneratedHash(expectedHash);

            var response = await _fixture.Handle(123, 2323, "Message1");

            Assert.AreEqual(expectedHash, response.Reference);
            Assert.AreEqual(_fixture.CohortId, response.Id);
        }
    }

    public class AddCohortCommandWithOtherPartyHandlerTestsFixture
    {
        public ProviderCommitmentsDbContext Db { get; set; }
        public long CohortId { get; set; }
        public Cohort Cohort { get; set; }

        public AddCohortCommandWithOtherPartyHandlerTestsFixture()
        {
            Db = new ProviderCommitmentsDbContext(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>()
                                                    .UseInMemoryDatabase(Guid.NewGuid().ToString())
                                                    .ConfigureWarnings(warnings => warnings.Throw(RelationalEventId.QueryClientEvaluationWarning))
                                                    .Options);
            CohortId = 8768;
            Cohort = new Cohort{Id = CohortId};

            EncodingServiceMock = new Mock<IEncodingService>();

            CohortDomainServiceMock = new Mock<ICohortDomainService>();
            CohortDomainServiceMock.Setup(x => x.CreateCohortWithOtherParty(It.IsAny<long>(), It.IsAny<long>(), 
                    It.IsAny<string>(), It.IsAny<UserInfo>(), It.IsAny<CancellationToken>()))
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

        public async Task<AddCohortResponse> Handle(long accountLegalEntity, long providerId, string message)
        {
            Db.SaveChanges();
            var command = new AddCohortWithOtherPartyCommand
            {
                AccountLegalEntityId = accountLegalEntity,
                ProviderId = providerId,
                Message = message,
                UserInfo = UserInfo
            };

            var handler = new AddCohortWithOtherPartyHandler(new Lazy<ProviderCommitmentsDbContext>(() => Db),
                EncodingService,
                Mock.Of<ILogger<AddCohortWithOtherPartyHandler>>(),
                CohortDomainServiceMock.Object);

            var response = await handler.Handle(command, CancellationToken.None);
            await Db.SaveChangesAsync();

            return response;
        }
    }
}