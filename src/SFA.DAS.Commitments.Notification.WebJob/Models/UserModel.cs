using System.Collections.Generic;

using SFA.DAS.EAS.Account.Api.Types;

namespace SFA.DAS.Commitments.Notification.WebJob.Models
{
    internal class UserModel
    {
        public long AccountId { get; set; }

        public IEnumerable<TeamMemberViewModel> Users { get; set; }

    }
}