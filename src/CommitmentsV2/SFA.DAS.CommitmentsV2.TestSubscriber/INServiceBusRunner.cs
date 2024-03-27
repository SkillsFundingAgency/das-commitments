using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.TestSubscriber;

public interface INServiceBusRunner
{
    Task StartNServiceBusBackgroundTask(string connectionString);
}