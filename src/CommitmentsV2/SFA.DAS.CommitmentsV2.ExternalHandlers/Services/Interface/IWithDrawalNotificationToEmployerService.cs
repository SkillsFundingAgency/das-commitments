using System.Threading.Tasks;
using NServiceBus;

namespace SFA.DAS.CommitmentsV2.ExternalHandlers.Services.Interface;

public interface IWithDrawalNotificationToEmployerService
{
    Task SendWithdrawalNotificationToEmployer(long apprenticeshipId, IMessageHandlerContext context);
}