using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Application.Commands.AddCohort;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Mapping;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.Encoding;
using SFA.DAS.Testing;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Commands
{
    [TestFixture]
    [Parallelizable]
    public class AddCohortCommandHandlerTests : FluentTest<AddCohortCommandHandlerTestFixture>
    {
        [Test]
        public async Task ShouldCreateCohort()
        {
            const string expectedHash = "ABC123";

            const long providerId = 1;
            const long accountId = 2;
            const long accountLegalEntityId = 3;
            long? transferSenderId = 4;
            int? pledgeApplicationId = 5;

            var fixtures = new AddCohortCommandHandlerTestFixture()
                                .WithGeneratedHash(expectedHash);

            var response = await fixtures.Handle(accountId, accountLegalEntityId, providerId, transferSenderId, pledgeApplicationId, "Course1");

            fixtures.CohortDomainServiceMock.Verify(x => x.CreateCohort(providerId, accountId, accountLegalEntityId, transferSenderId, pledgeApplicationId,
                It.IsAny<DraftApprenticeshipDetails>(),
                fixtures.UserInfo,
                It.IsAny<CancellationToken>()));

            Assert.AreEqual(expectedHash, response.Reference);
        }
    }

    public class TestLogger : ILogger<AddCohortHandler>
    {
        private readonly List<(LogLevel logLevel, Exception exception, string message)> _logMessages = new List<(LogLevel logLevel, Exception exception, string message)>();

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            _logMessages.Add((logLevel, exception, formatter(state, exception)));
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            throw new NotImplementedException();
        }

        public bool HasErrors => _logMessages.Any(l => l.logLevel == LogLevel.Error);
        public bool HasInfo => _logMessages.Any(l => l.logLevel == LogLevel.Information);
    }

    public class AddCohortCommandHandlerTestFixture
    {
        public ProviderCommitmentsDbContext Db { get; set; }

        public Mock<Provider> Provider { get; set; }

        public AddCohortCommandHandlerTestFixture()
        {
            Db = new ProviderCommitmentsDbContext(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>()
                                                    .UseInMemoryDatabase(Guid.NewGuid().ToString())
                                                    .Options);

            EncodingServiceMock = new Mock<IEncodingService>();

            DraftApprenticeshipDetailsMapperMock =
                new Mock<IOldMapper<AddCohortCommand, DraftApprenticeshipDetails>>();
            DraftApprenticeshipDetailsMapperMock.Setup(x => x.Map(It.IsAny<AddCohortCommand>()))
                .ReturnsAsync(() => new DraftApprenticeshipDetails());

            var commitment = new Cohort();
            commitment.Apprenticeships.Add(new DraftApprenticeship());

            CohortDomainServiceMock = new Mock<ICohortDomainService>();
            CohortDomainServiceMock.Setup(x => x.CreateCohort(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<long>(), It.IsAny<long?>(), It.IsAny<int?>(),
                    It.IsAny<DraftApprenticeshipDetails>(), It.IsAny<UserInfo>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(commitment);

            Logger = new TestLogger();
            UserInfo = new UserInfo();
        }

        public Mock<IEncodingService> EncodingServiceMock { get; }
        public IEncodingService EncodingService => EncodingServiceMock.Object;

        public Mock<IOldMapper<AddCohortCommand, DraftApprenticeshipDetails>> DraftApprenticeshipDetailsMapperMock { get; }

        public Mock<ICohortDomainService> CohortDomainServiceMock { get; }

        public TestLogger Logger { get; }
        public UserInfo UserInfo { get; }

        public AddCohortCommandHandlerTestFixture WithGeneratedHash(string hash)
        {
            EncodingServiceMock
                .Setup(hs => hs.Encode(It.IsAny<long>(), It.Is<EncodingType>(encoding => encoding == EncodingType.CohortReference)))
                .Returns(hash);

            return this;
        }

        public async Task<AddCohortResult> Handle(long accountId, long accountLegalEntity, long providerId, long? transferSenderId, int? pledgeApplicationId, string courseCode)
        {
            Db.SaveChanges();
            
            var command = new AddCohortCommand(
                accountId,
                accountLegalEntity,
                providerId,
                courseCode, 
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                transferSenderId,
                pledgeApplicationId,
                UserInfo);

            var handler = new AddCohortHandler(new Lazy<ProviderCommitmentsDbContext>(() => Db),
                EncodingService,
                Logger,
                DraftApprenticeshipDetailsMapperMock.Object,
                CohortDomainServiceMock.Object);

            var response = await handler.Handle(command, CancellationToken.None);
            await Db.SaveChangesAsync();

            return response;
        }
    }
}