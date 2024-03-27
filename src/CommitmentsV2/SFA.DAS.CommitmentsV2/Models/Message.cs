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
            CreatedBy = ConvertSendingPartyToCreatedBy(sendingParty);
            CreatedDateTime = DateTime.UtcNow;
            Text = text;
        }

        private byte ConvertSendingPartyToCreatedBy(Party sendingParty)
        {
            switch (sendingParty)
            {
                case Party.Employer: return 0;
                case Party.Provider: return 1;
                default:
                    throw new ArgumentException($"Cannot create message from {sendingParty}");
            }
        }
    }
}