using System.Threading.Tasks;
using NServiceBus.ObjectBuilder.Common;

namespace SFA.DAS.CommitmentsV2.TestSubscriber
{
    public interface INServiceBusRunner
    {
        Task StartNServiceBusBackgroundTask(string connectionString);
    }
}