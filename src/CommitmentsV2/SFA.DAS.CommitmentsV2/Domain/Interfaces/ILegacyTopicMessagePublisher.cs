namespace SFA.DAS.CommitmentsV2.Domain.Interfaces
{
    public interface ILegacyTopicMessagePublisher
    {
        Task PublishAsync<T>(T message);
    }
}