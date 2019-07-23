using System;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Models
{
    public class Message
    {
        public long Id { get; set; }
        public long CommitmentId { get; set; }
        public string Text { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public string Author { get; set; }
        public byte CreatedBy { get; set; }

        public virtual Cohort Cohort { get; set; }

        public Message()
        {

        }

        public Message(Cohort cohort, Party sendingParty, string author, string text)
        {
            Author = author;
            CreatedBy = sendingParty == Party.Employer ? (byte)0 : (byte)1; //todo: make this nicer
            CreatedDateTime = DateTime.UtcNow;
            Text = text;
        }
    }
}