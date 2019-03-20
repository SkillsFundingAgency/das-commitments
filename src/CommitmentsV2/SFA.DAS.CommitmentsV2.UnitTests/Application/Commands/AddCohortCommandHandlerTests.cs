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
using SFA.DAS.Apprenticeships.Api.Client;
using SFA.DAS.Apprenticeships.Api.Types;
using SFA.DAS.CommitmentsV2.Application.Commands.AddCohort;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Exceptions;
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
        public async Task Handle_WhenHandlingValidCommand_ThenShouldCreateAccountAndLogSuccess()
        {
            const string expectedHash = "ABC123";

            var fixtures = new AddCohortCommandHandlerTestFixture()
                                .WithGeneratedHash(expectedHash)
                                .WithProvider(1)
                                .WithAccountLegalEntity(2, 3)
                                .WithCourse("Course1");

            var response = await fixtures.Handle(3, 1, "Course1");

            Assert.IsNotNull(response);
            Assert.AreNotEqual(0, response.Id);
            Assert.AreEqual(expectedHash, response.Reference);
             
            Assert.IsTrue(fixtures.Logger.HasInfo);
            Assert.IsFalse(fixtures.Logger.HasErrors);
        }

        [Test]
        public void Handle_WhenHandlingInvalidCommand_ThenShouldThrowExceptionAndLogFailure()
        {
            const string expectedHash = "ABC123";

            var fixtures = new AddCohortCommandHandlerTestFixture()
                .WithGeneratedHash(expectedHash)
                .WithCourse("Course1"); 

            Assert.ThrowsAsync<BadRequestException>(() => fixtures.Handle(3, 1, "Course1"));
            Assert.IsFalse(fixtures.Logger.HasInfo);
            Assert.IsTrue(fixtures.Logger.HasErrors);
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

        public AddCohortCommandHandlerTestFixture()
        {
            Db = new ProviderCommitmentsDbContext(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>()
                                                    .UseInMemoryDatabase(Guid.NewGuid().ToString())
                                                    .ConfigureWarnings(warnings => warnings.Throw(RelationalEventId.QueryClientEvaluationWarning))
                                                    .Options);

            HashingServiceMock = new Mock<IHashingService>();

            TrainingProgrammeApiClientMock = new Mock<ITrainingProgrammeApiClient>();

            Logger = new TestLogger(); 
        }

        public Mock<IHashingService> HashingServiceMock { get; }
        public IHashingService HashingService => HashingServiceMock.Object;

        public Mock<ITrainingProgrammeApiClient> TrainingProgrammeApiClientMock { get; }
        public ITrainingProgrammeApiClient TrainingProgrammeApiClient => TrainingProgrammeApiClientMock.Object;


        public TestLogger Logger { get; }

        public AddCohortCommandHandlerTestFixture WithGeneratedHash(string hash)
        {
            HashingServiceMock
                .Setup(hs => hs.HashValue(It.IsAny<long>()))
                .Returns(hash);

            return this;
        }

        public AddCohortCommandHandlerTestFixture WithCourse(string courseCode)
        {
            TrainingProgrammeApiClientMock
                .Setup(tp => tp.GetTrainingProgramme(courseCode))
                .ReturnsAsync(() => new Framework
                {
                    FrameworkId = courseCode,
                    FrameworkName = $"Framework {courseCode}",
                    PathwayName = $"Pathway {courseCode}",
                    Title = $"Course {courseCode}"
                });

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

        public AddCohortCommandHandlerTestFixture WithProvider(long providerId)
        {
            var provider = new Provider{UkPrn = providerId, Name=$"Provider {providerId:D3}"};

            Db.Providers.Add(provider);

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

            var handler = new AddCohortHandler(new Lazy<ProviderCommitmentsDbContext>(() => Db), HashingService, Logger, TrainingProgrammeApiClient);
            var response = await handler.Handle(command, CancellationToken.None);
            await Db.SaveChangesAsync();

            return response;
        }
    }
}
