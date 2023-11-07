using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Application.Commands.AddFileUploadLog;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Models;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Commands.BulkUpload
{
    [TestFixture]
    [Parallelizable]
    public class AddFileUploadCommandHandlerTests2 
    {
        [Test]
        public async Task Handle_WhenCommandIsHandled_ThenShouldCreateLog()
        {
            var f = new AddFileUploadCommandHandlerTestsFixture();
            await f.Handle();
            var log = f.Db.FileUploadLogs.FirstOrDefault();
            log.Should().NotBeNull();
            log.ProviderId.Should().Be(2068);
            log.RplCount.Should().Be(10);
            log.RowCount.Should().Be(100);
            log.FileContent.Should().Be("contents");
            log.FileName.Should().Be("filename");
        }

        [Test]
        public async Task Handle_WhenCommandIsHandled_ThenShouldReturnNexytId()
        {
            var f = new AddFileUploadCommandHandlerTestsFixture();
            var result = await f.Handle();
            result.Should().NotBeNull();
            result.LogId.Should().BeGreaterThan(0);
        }
    }

    public class AddFileUploadCommandHandlerTestsFixture
    {
        public FileUploadLog Log { get; set; }
        public AddFileUploadLogCommand Command { get; set; }
        public IRequestHandler<AddFileUploadLogCommand, BulkUploadAddLogResponse> Handler { get; set; }
        public ProviderCommitmentsDbContext Db { get; set; }

        public AddFileUploadCommandHandlerTestsFixture()
        {
            Command = new AddFileUploadLogCommand
            {
                ProviderId = 2068,
                RplCount = 10,
                RowCount = 100,
                FileContent = "contents",
                FileName = "filename"
            };

            Db = new ProviderCommitmentsDbContext(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

            Handler = new AddFileUploadCommandHandler(new Lazy<ProviderCommitmentsDbContext>(() => Db));
        }

        public async Task<BulkUploadAddLogResponse> Handle()
        {
            return await Handler.Handle(Command, CancellationToken.None);
        }
    }
}
