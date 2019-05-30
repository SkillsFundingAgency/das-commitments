using SFA.DAS.Commitments.Application.Interfaces;
using SFA.DAS.Messaging.Interfaces;
using System.Threading.Tasks;

namespace SFA.DAS.Commitments.Application.Services
{
    public class MessagePublisherWithV2Upgrade : IMessagePublisher
    {
        private readonly IMessagePublisher _messagePublisher;
        private readonly IEventConsumer _eventConsumer;

        public MessagePublisherWithV2Upgrade(IMessagePublisher messagePublisher, IEventConsumer eventConsumer)
        {
            _messagePublisher = messagePublisher;
            _eventConsumer = eventConsumer;
        }

        public async Task PublishAsync(object message)
        {
            await _eventConsumer.Consume(message);

            await _messagePublisher.PublishAsync(message);
        }
    }
}
