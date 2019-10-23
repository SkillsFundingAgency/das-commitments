using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.Domain.Interfaces
{
    public interface ILegacyTopicMessagePublisher
    {
        Task PublishAsync(object message);
    }
}