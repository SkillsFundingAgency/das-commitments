using System;
using SFA.DAS.NServiceBus;

namespace SFA.DAS.EmployerAccounts.Messages.Events
{
    public class DeletedTransferConnectionRequestEvent : Event
    {
        public long DeletedByAccountId { get; set; }
        public long DeletedByUserId { get; set; }
        public string DeletedByUserName { get; set; }
        public Guid DeletedByUserRef { get; set; }
        public string ReceiverAccountHashedId { get; set; }
        public long ReceiverAccountId { get; set; }
        public string ReceiverAccountName { get; set; }
        public string SenderAccountHashedId { get; set; }
        public long SenderAccountId { get; set; }
        public string SenderAccountName { get; set; }
        public int TransferConnectionRequestId { get; set; }
    }
}
