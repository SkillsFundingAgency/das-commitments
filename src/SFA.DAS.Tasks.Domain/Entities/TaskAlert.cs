using System;

namespace SFA.DAS.Tasks.Domain.Entities
{
    public class TaskAlert
    {
        public long Id { get; set; }
        public long TaskId { get; set; }
        public string UserId { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}
