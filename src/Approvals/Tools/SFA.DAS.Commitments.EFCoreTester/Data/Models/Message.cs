using System;

namespace SFA.DAS.Commitments.EFCoreTester.Data.Models
{
    public partial class Message
    {
        public long Id { get; set; }
        public long CommitmentId { get; set; }
        public string Text { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public string Author { get; set; }
        public byte CreatedBy { get; set; }

        public virtual Commitment Commitment { get; set; }
    }
}
