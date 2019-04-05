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
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Domain.ValueObjects;
using SFA.DAS.CommitmentsV2.Mapping;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.HashingService;
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

            long accountId = 2;
            long accountLegalEntityId = 3;

            var fixtures = new AddCohortCommandHandlerTestFixture()
                                .WithGeneratedHash(expectedHash)
                                .WithAccountLegalEntity(accountId, accountLegalEntityId);

            var response = await fixtures.Handle(3, 1, "Course1");

            fixtures.Provider.Verify(x =>
                    x.CreateCohort(It.Is<AccountLegalEntity>(ale =>
                            ale.AccountId == accountId && ale.Id == accountLegalEntityId),
                        It.IsAny<DraftApprenticeshipDetails>(), //todo be more specific
                        It.IsAny<IUlnValidator>(), It.IsAny<ICurrentDateTime>(), It.IsAny<IAcademicYearDateProvider>()),
                Times.Once);

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
                                                    .ConfigureWarnings(warnings => warnings.Throw(RelationalEventId.QueryClientEvaluationWarning))
                                                    .Options);

            HashingServiceMock = new Mock<IHashingService>();

            DraftApprenticeshipDetailsMapperMock =
                new Mock<IAsyncMapper<AddCohortCommand, DraftApprenticeshipDetails>>();
            DraftApprenticeshipDetailsMapperMock.Setup(x => x.Map(It.IsAny<AddCohortCommand>()))
                .ReturnsAsync(() => new DraftApprenticeshipDetails());

            var commitment = new Commitment();
            commitment.Apprenticeship.Add(new DraftApprenticeship());

            Provider = new Mock<Provider>();
            Provider.Setup(
                x => x.CreateCohort(It.IsAny<AccountLegalEntity>(),
                        It.IsAny<DraftApprenticeshipDetails>(),
                        It.IsAny<IUlnValidator>(),
                        It.IsAny<ICurrentDateTime>(),
                        It.IsAny<IAcademicYearDateProvider>()
                    ))
                .Returns(commitment);

            Db.Providers.Add(Provider.Object);

            Logger = new TestLogger(); 
        }

        public Mock<IHashingService> HashingServiceMock { get; }
        public IHashingService HashingService => HashingServiceMock.Object;

        public Mock<IAsyncMapper<AddCohortCommand,DraftApprenticeshipDetails>> DraftApprenticeshipDetailsMapperMock { get; }


        public TestLogger Logger { get; }

        public AddCohortCommandHandlerTestFixture WithGeneratedHash(string hash)
        {
            HashingServiceMock
                .Setup(hs => hs.HashValue(It.IsAny<long>()))
                .Returns(hash);

            return this;
        }

        public AddCohortCommandHandlerTestFixture WithAccountLegalEntity(long accountId, long accountLegalEntityId)
        {
            var account = new Account(accountId, $"PRI{accountId:D3}", $"PUB{accountId:D3}", "Account {accountId}",
                DateTime.Now);

            account.AddAccountLegalEntity(accountLegalEntityId,
                $"PUB{accountLegalEntityId:D3}",
                $"ALE {accountLegalEntityId}",
                $"AccountLegalEntityResponse {accountLegalEntityId:D3}",
                OrganisationType.Charities,
                "High Street", DateTime.Now);

            Db.Accounts.Add(account);

            return this;
        }

        public async Task<AddCohortResponse> Handle(long accountLegalEntity, long providerId, string courseCode)
        {
            Db.SaveChanges();
            var command = new AddCohortCommand
            {
                AccountLegalEntityId = accountLegalEntity,
                ProviderId = providerId,
                ReservationId = Guid.NewGuid(),
                CourseCode = courseCode
            };

            var handler = new AddCohortHandler(new Lazy<ProviderCommitmentsDbContext>(() => Db),
                HashingService,
                Logger,
                DraftApprenticeshipDetailsMapperMock.Object,
                Mock.Of<IUlnValidator>(),
                Mock.Of<ICurrentDateTime>(),
                Mock.Of<IAcademicYearDateProvider>());

            var response = await handler.Handle(command, CancellationToken.None);
            await Db.SaveChangesAsync();

            return response;
        }
    }
}
