using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Application.Commands.AddCohort;
using SFA.DAS.CommitmentsV2.Application.Commands.AddHistory;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.Encoding;
using SFA.DAS.Testing;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Commands
{

    [TestFixture]
    public class AddHistoryCommandHandlerTests : FluentTest<AddHistoryCommandHandlerTestsFixture>
    {
        private AddHistoryCommandHandlerTestsFixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new AddHistoryCommandHandlerTestsFixture();
        }

        [Test]
        public async Task ShouldUseCommandApprenticeshipIdWhenNotNull()
        {
            var command = _fixture.CreateAddHistoryCommand();
            command.ApprenticeshipId = _fixture.ApprenticeshipId;

            await _fixture.Handle(command);
            await _fixture.VerifyValidApprenticeshipIdIsUsed(command.ApprenticeshipId.GetValueOrDefault());
        }

        [Test]
        [TestCase(nameof(DraftApprenticeship))]
        [TestCase(nameof(DraftApprenticeship))]
        public async Task ShouldUseCommandEntityIdAsApprenticeshipIdWhenApprenticeshipIdIsNull(string entityType)
        {
            var command = _fixture.CreateAddHistoryCommand();
            command.ApprenticeshipId = null;
            command.EntityId = 1012;
            command.EntityType = entityType;

            await _fixture.Handle(command);
            await _fixture.VerifyValidApprenticeshipIdIsUsed(command.EntityId);
        }

        [Test]
        public async Task ShouldSetApprenticeshipIdToNullWhenApprenticeshipIdIsNullAndTypeIsNotApprenticeship()
        {
            var command = _fixture.CreateAddHistoryCommand();
            command.ApprenticeshipId = null;
            command.EntityId = 1012;
            command.EntityType = "PriceEpisode";

            await _fixture.Handle(command);
            await _fixture.VerifyApprenticeshipIdIsNull();
        }
    }

    public class AddHistoryCommandHandlerTestsFixture
    {
        public ProviderCommitmentsDbContext Db { get; set; }

        public IRequestHandler<AddHistoryCommand> Handler { get; set; }
        public Fixture Fixture { get; set; }
        public Apprenticeship ApprenticeshipDetails { get; set; }
        public long ApprenticeshipId { get; set; }

        public AddHistoryCommandHandlerTestsFixture()
        {
            Db = new ProviderCommitmentsDbContext(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>()
                                                    .UseInMemoryDatabase(Guid.NewGuid().ToString())
                                                    .ConfigureWarnings(warnings => warnings.Throw(RelationalEventId.QueryClientEvaluationWarning))
                                                    .Options);
            Fixture = new Fixture();
            ApprenticeshipId = Fixture.Create<long>();
        }

        public async Task Handle(AddHistoryCommand command)
        {
            Handler = new AddHistoryCommandHandler(Db);
            var response = await Handler.Handle(command, CancellationToken.None);
            await Db.SaveChangesAsync();
        }

        public async Task CreateApprenticeship()
        {
            ApprenticeshipDetails = Fixture.Build<CommitmentsV2.Models.Apprenticeship>()
                .With(s => s.Id, ApprenticeshipId)
                .Without(s => s.PriceHistory)
                .Without(s => s.ApprenticeshipUpdate)
                .Without(s => s.DataLockStatus)
                .Without(s => s.EpaOrg)
                .Without(s => s.Continuation)
                .Without(s => s.PreviousApprenticeship)
                .Create();

            await Db.SaveChangesAsync();
        }

        public AddHistoryCommand CreateAddHistoryCommand()
        {
            return Fixture.Build<AddHistoryCommand>().Create();
        }

        public async Task VerifyValidApprenticeshipIdIsUsed(long expectedApprenticeshipId)
        {
            await Db.SaveChangesAsync();

            var history = await Db.History.FirstOrDefaultAsync(x => x.ApprenticeshipId == expectedApprenticeshipId);
            Assert.IsNotNull(history);
        }

        public async Task VerifyApprenticeshipIdIsNull()
        {
            await Db.SaveChangesAsync();

            var history = await Db.History.FirstOrDefaultAsync();
            Assert.IsNull(history.ApprenticeshipId);
        }
    }
}
