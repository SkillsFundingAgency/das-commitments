using System.Threading.Tasks;

namespace SFA.DAS.Commitments.Application.Interfaces
{
    public interface IEventUpgradeHandler<in T> where T : class
    {
        Task Execute(T @event);
    }
}
