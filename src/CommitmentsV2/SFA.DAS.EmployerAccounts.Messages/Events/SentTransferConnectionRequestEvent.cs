using System;
using SFA.DAS.NServiceBus;

namespace SFA.DAS.EmployerAccounts.Messages.Events
{
    public class SentTransferConnectionRequestEvent : Event
    {
        public string ReceiverAccountHashedId { get; set; }
        public long ReceiverAccountId { get; set; }
        public string ReceiverAccountName { get; set; }
        public string SenderAccountHashedId { get; set; }
        public long SenderAccountId { get; set; }
        public string SenderAccountName { get; set; }
        public long SentByUserId { get; set; }
        public string SentByUserName { get; set; }
        public Guid SentByUserRef { get; set; }
        public int TransferConnectionRequestId { get; set; }
    }
}
