﻿using SFA.DAS.CommitmentsV2.Application.Commands.AddHistory;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Models;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Commands
{
    [TestFixture]
    public class AddHistoryCommandHandlerTests
    {
        private AddHistoryCommandHandlerTestsFixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new AddHistoryCommandHandlerTestsFixture();
        }
        
        [TearDown]
        public void TearDown()
        {
            _fixture?.Dispose();
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

    public class AddHistoryCommandHandlerTestsFixture : IDisposable
    {
        public ProviderCommitmentsDbContext Db { get; set; }

        public IRequestHandler<AddHistoryCommand> Handler { get; set; }
        public Fixture Fixture { get; set; }
        public Apprenticeship ApprenticeshipDetails { get; set; }
        public long ApprenticeshipId { get; set; }

        public AddHistoryCommandHandlerTestsFixture()
        {
            Db = new ProviderCommitmentsDbContext(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>()
                                                    .UseInMemoryDatabase(Guid.NewGuid().ToString(), b => b.EnableNullChecks(false))
                                                    .Options);
            Fixture = new Fixture();
            ApprenticeshipId = Fixture.Create<long>();
        }

        public async Task Handle(AddHistoryCommand command)
        {
            Handler = new AddHistoryCommandHandler(Db);
            await Handler.Handle(command, CancellationToken.None);
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
            Assert.That(history, Is.Not.Null);
        }

        public async Task VerifyApprenticeshipIdIsNull()
        {
            await Db.SaveChangesAsync();

            var history = await Db.History.FirstOrDefaultAsync();
            Assert.That(history.ApprenticeshipId, Is.Null);
        }

        public void Dispose()
        {
            Db?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
