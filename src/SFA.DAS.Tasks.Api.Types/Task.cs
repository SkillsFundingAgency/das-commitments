using System;

namespace SFA.DAS.Tasks.Api.Types
{
    public class Task
    {
        public long Id { get; set; }
        public long TemplateId { get; set; }
        public string Name { get; set; }
        public TaskStatuses TaskStatus { get; set; }
        public DateTime Created { get; set; }
        public DateTime Completed { get; set; }
        public string CompletedBy { get; set; }
    }
}
