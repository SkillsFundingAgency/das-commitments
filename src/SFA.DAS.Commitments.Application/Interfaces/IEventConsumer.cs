using System;
using System.Threading.Tasks;

namespace SFA.DAS.Commitments.Application.Interfaces
{
    public interface IEventConsumer
    {
        void RegisterHandler<T>(Func<T, Task> handler) where T : class;

        Task Consume<T>(T message) where T : class;
    }
}
