using SFA.DAS.Commitments.Domain.Entities.DataLock;
using SFA.DAS.Provider.Events.Api.Types;

namespace SFA.DAS.Commitments.Infrastructure.Services
{
    public interface IPaymentEventMapper
    {
        DataLockStatus Map(DataLockEvent result);
    }
}
