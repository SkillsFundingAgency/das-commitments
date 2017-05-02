using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFA.DAS.Commitments.Domain.Entities.History
{
    public class HistoryItem
    {
        public string EntityType { get; set; }
        public long EntityId { get; set; }
        public string UserId { get; set; }
        public string UpdatedByRole { get; set; }
        public string ChangeType { get; set; }
        public string UpdatedByName { get; set; }
        public string OriginalState { get; set; }
        public string UpdatedState { get; set; }
    }
}
