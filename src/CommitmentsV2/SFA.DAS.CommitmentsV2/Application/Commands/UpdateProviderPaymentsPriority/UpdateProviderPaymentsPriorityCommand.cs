using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Application.Commands.UpdateProviderPaymentsPriority
{
    public class UpdateProviderPaymentsPriorityCommand : IRequest
    {
        public long AccountId { get; }
        public List<ProviderPaymentPriorityUpdateItem> ProviderPaymentPriorityUpdateItems { get; }
        public UserInfo UserInfo { get; }

        public UpdateProviderPaymentsPriorityCommand(long accountId, List<ProviderPaymentPriorityUpdateItem> providerPaymentPriorityUpdateItems, UserInfo userInfo)
        {
            AccountId = accountId;
            ProviderPaymentPriorityUpdateItems = providerPaymentPriorityUpdateItems;
            UserInfo = userInfo ?? throw new ArgumentNullException(nameof(userInfo));
        }

        public sealed class ProviderPaymentPriorityUpdateItem
        {
            public long ProviderId { get; set; }
            public int PriorityOrder { get; set; }
        }
    }
}