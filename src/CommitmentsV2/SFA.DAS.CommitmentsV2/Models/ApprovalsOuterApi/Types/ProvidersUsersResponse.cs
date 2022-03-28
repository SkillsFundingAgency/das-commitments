using System;
using System.Collections.Generic;
using System.Text;

namespace SFA.DAS.CommitmentsV2.Models.ApprovalsOuterApi.Types
{
    public class GetProviderUsersListItem
    {
        public string UserRef { get; set; }
        public string EmailAddress { get; set; }
        public string DisplayName { get; set; }
        public bool ReceiveNotifications { get; set; }
        public bool IsSuperUser { get; set; }
    }

    public class ProvidersUsersResponse
    {
        public IEnumerable<GetProviderUsersListItem> Users { get; set; }

    }
}
