using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Application.Commands.FileUploadLogUpdateWithErrorContent;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Models;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Commands
{
    [TestFixture]
    [Parallelizable]
    public class FileUploadLogUpdateWithErrorContentCommandHandlerTests
    {

        [Test]
        public async Task UpdateErrorContent_ShouldSucceed()
        {
            var f = new FileUploadLogUpdateWithErrorContentCommandHandlerTestsFixture();
            f.WithExistingLog();
            await f.Handle();
            f.VerifyLogIsUpdatedCorrectly();
        }

        [Test]
        public async Task UpdateErrorContent_ShouldFailIfIncorrectProviderIdIsSentIn()
        {
            var f = new FileUploadLogUpdateWithErrorContentCommandHandlerTestsFixture();
            f.WithExistingLog();
            f.Command.ProviderId++;
            try
            {
                await f.Handle();
            }
            catch (Exception ex)
            {
                Assert.That(ex.Message, Is.EqualTo($"Incorrect Provider {f.Command.ProviderId} specified for FileUpload Id {f.Command.LogId}"));
            }
        }

        [Test]
        public async Task UpdateErrorContent_ShouldFailIfNoLogFound()
        {
            var f = new FileUploadLogUpdateWithErrorContentCommandHandlerTestsFixture();
            try
            {
                await f.Handle();
            }
            catch (Exception ex)
            {
                Assert.That(ex.Message, Is.EqualTo($"No FileLogUpload entry found for Id {f.Command.LogId}"));
            }
        }
    }

    public class FileUploadLogUpdateWithErrorContentCommandHandlerTestsFixture
    {
        public FileUploadLogUpdateWithErrorContentCommand Command { get; private set; }
        public IRequestHandler<FileUploadLogUpdateWithErrorContentCommand> Sut { get; }
        public ProviderCommitmentsDbContext Db { get; }
        public Mock<ILogger<FileUploadLogUpdateWithErrorContentCommandHandler>> Logger { get; private set; }

        public FileUploadLog LogEntry { get; private set; }

        private Fixture _autoFixture;

        public FileUploadLogUpdateWithErrorContentCommandHandlerTestsFixture()
        {
            _autoFixture = new Fixture();
            Logger = new Mock<ILogger<FileUploadLogUpdateWithErrorContentCommandHandler>>();
            LogEntry = _autoFixture.Build<FileUploadLog>().Without(x => x.Error).Without(x => x.CohortLogs).Create();

            Command = _autoFixture.Build<FileUploadLogUpdateWithErrorContentCommand>()
                .With(x => x.LogId, LogEntry.Id)
                .With(x => x.ProviderId, LogEntry.ProviderId).Create();

            Db = new ProviderCommitmentsDbContext(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString(), b => b.EnableNullChecks(false)).Options);

            Sut = new FileUploadLogUpdateWithErrorContentCommandHandler(new Lazy<ProviderCommitmentsDbContext>(() => Db), Logger.Object);
        }

        public async Task Handle()
        {
            Db.SaveChanges();

            await Sut.Handle(Command, CancellationToken.None);
            await Db.SaveChangesAsync();
        }

        public FileUploadLogUpdateWithErrorContentCommandHandlerTestsFixture WithExistingLog()
        {
            Db.FileUploadLogs.Add(LogEntry);
            return this;
        }

        public void VerifyLogIsUpdatedCorrectly()
        {
            var first = Db.FileUploadLogs.First();
            first.Error.Should().Be(Command.ErrorContent);
        }
    }
}
