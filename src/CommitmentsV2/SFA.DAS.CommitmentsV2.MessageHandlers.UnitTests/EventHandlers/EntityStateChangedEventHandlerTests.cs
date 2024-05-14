using KellermanSoftware.CompareNetObjects;
using Newtonsoft.Json;
using SFA.DAS.CommitmentsV2.Application.Commands.AddHistory;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers;
using SFA.DAS.CommitmentsV2.Messages.Events;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.UnitTests.EventHandlers
{
    [TestFixture]
    public class EntityStateChangedEventHandlerTests
    {
        private EntityStateChangedEventHandlerTestsFixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new EntityStateChangedEventHandlerTestsFixture();
        }

        [Test]
        public async Task Handle_When_Called_Differences_Are_Determined()
        {
            await _fixture.Handle();
            _fixture.VerifyDifferencesAreDetermined();
        }

        [Test]
        public async Task Handle_When_Called_And_Differences_Are_Determined_Then_History_Is_Recorded()
        {
            _fixture.WithDifferences();
            await _fixture.Handle();
            _fixture.VerifyHistoryIsRecorded();
        }

        [Test]
        public async Task Handle_When_Called_And_No_Differences_Are_Determined_Then_History_Is_Not_Recorded()
        {
            await _fixture.Handle();
            _fixture.VerifyHistoryIsNotRecorded();
        }

        private class EntityStateChangedEventHandlerTestsFixture
        {
            private readonly EntityStateChangedEventHandler _handler;
            private readonly EntityStateChangedEvent _message;
            private readonly Mock<IMediator> _mediator;
            private readonly Mock<IDiffService> _diffService;
            private readonly List<DiffItem> _differences;

            private readonly Dictionary<string, object> _initialState;
            private readonly Dictionary<string, object> _updatedState; 

            public EntityStateChangedEventHandlerTestsFixture()
            {
                _mediator = new Mock<IMediator>();

                _differences = new List<DiffItem>();
                _diffService = new Mock<IDiffService>();
                _diffService.Setup(x =>
                        x.GenerateDiff(It.IsAny<Dictionary<string, object>>(), It.IsAny<Dictionary<string, object>>()))
                    .Returns(_differences.AsReadOnly);

                _initialState = new Dictionary<string, object> {{"test", "initial"}};
                _updatedState = new Dictionary<string, object> {{"test", "updated"}};

                _message = new EntityStateChangedEvent {InitialState = JsonConvert.SerializeObject(_initialState), UpdatedState = JsonConvert.SerializeObject(_updatedState)};

                _handler = new EntityStateChangedEventHandler(_mediator.Object, _diffService.Object);
            }

            public async Task Handle()
            {
                await _handler.Handle(_message, Mock.Of<IMessageHandlerContext>());
            }

            public EntityStateChangedEventHandlerTestsFixture WithDifferences()
            {
                _differences.Add(new DiffItem {PropertyName = "FirstName", InitialValue = "Foo", UpdatedValue = "Bar"});
                return this;
            }

            public EntityStateChangedEventHandlerTestsFixture VerifyDifferencesAreDetermined()
            {
                var logic = new CompareLogic();

                _diffService.Verify(x => x.GenerateDiff(
                    It.Is<Dictionary<string, object>>(initial => logic.Compare(initial, _initialState).AreEqual),
                    It.Is<Dictionary<string, object>>(updated => logic.Compare(updated, _updatedState).AreEqual)));
                return this;
            }

            public void VerifyHistoryIsRecorded()
            {
                _mediator.Verify(x=> x.Send(It.Is<AddHistoryCommand>(history =>
                    history.CorrelationId == _message.CorrelationId
                    && history.EntityId == _message.EntityId
                    && history.StateChangeType == _message.StateChangeType
                    && history.EntityType == _message.EntityType
                    && history.EmployerAccountId == _message.EmployerAccountId
                    && history.ProviderId == _message.ProviderId
                    && history.InitialState == _message.InitialState
                    && history.UpdatedState == _message.UpdatedState
                    && history.Diff == JsonConvert.SerializeObject(_differences)
                    && history.UpdatingUserId == _message.UpdatingUserId
                    && history.UpdatingUserName == _message.UpdatingUserName
                    && history.UpdatingParty == _message.UpdatingParty
                    && history.UpdatedOn == _message.UpdatedOn

                ), It.IsAny<CancellationToken>()), Times.Once);
            }

            public void VerifyHistoryIsNotRecorded()
            {
                _mediator.Verify(x => x.Send(It.IsAny<AddHistoryCommand>(), It.IsAny<CancellationToken>()), Times.Never);
            }
        }
    }
}
