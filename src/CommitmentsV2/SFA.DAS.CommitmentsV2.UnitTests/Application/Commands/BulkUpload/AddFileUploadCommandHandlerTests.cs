using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Application.Commands.AddFileUploadLog;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.Testing;
using SFA.DAS.Testing.Builders;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Commands
{
    [TestFixture]
    [Parallelizable]
    public class AddFileUploadCommandHandlerTests2 : FluentTest<AddFileUploadCommandHandlerTestsFixture>
    {
        [Test]
        public Task Handle_WhenCommandIsHandled_ThenShouldCreateLog()
        {
            return TestAsync(f => f.Handle(), f =>
            {
                f.Log.Id.Should().Be(1024);
                f.Log.ProviderId.Should().Be(2068);
                f.Log.RplCount.Should().Be(10);
                f.Log.RowCount.Should().Be(100);
                f.Log.FileContent.Should().Be("contents");
                f.Log.FileName.Should().Be("filename");
            });
        }
    }

    public class AddFileUploadCommandHandlerTestsFixture
    {
        public FileUploadLog Log { get; set; }
        public AddFileUploadLogCommand Command { get; set; }
        public IRequestHandler<AddFileUploadLogCommand, BulkUploadAddLogResponse> Handler { get; set; }
        public ProviderCommitmentsDbContext Db { get; set; }
        public long Id { get; set; }
        public long ProviderId { get; set; }
        public int RplCount { get; set; }
        public int RowCount { get; set; }
        public string FileContent { get; set; }
        public string FileName { get; set; }

        public AddFileUploadCommandHandlerTestsFixture()
        {

            Id = 1024;
            ProviderId = 2068;
            RplCount = 10;
            RowCount = 100;
            FileContent = "contents";
            FileName = "filename";

            Log = ObjectActivator.CreateInstance<FileUploadLog>()
                    .Set(a => a.Id, Id)
                    .Set(a => a.RplCount, RplCount)
                    .Set(a => a.RowCount, RowCount)
                    .Set(a => a.FileContent, FileContent)
                    .Set(a => a.FileName, FileName)
                    .Set(a => a.ProviderId, ProviderId);

            Command = new AddFileUploadLogCommand();
            Db = new ProviderCommitmentsDbContext(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

            Db.FileUploadLogs.Add(Log);
            Db.SaveChanges();

            Handler = new AddFileUploadCommandHandler(new Lazy<ProviderCommitmentsDbContext>(() => Db));
        }

        public async Task Handle()
        {
            await Handler.Handle(Command, CancellationToken.None);
            await Db.SaveChangesAsync();
        }
    }
}
