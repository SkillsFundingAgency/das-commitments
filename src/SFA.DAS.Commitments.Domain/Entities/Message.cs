using System;

namespace SFA.DAS.Commitments.Domain.Entities
{
    public class Message
    {
        public string Author { get; set; }
        public string Text { get; set; }
        public CallerType CreatedBy { get; set; }
        public DateTime CreatedDateTime { get; set; }
    }
}
