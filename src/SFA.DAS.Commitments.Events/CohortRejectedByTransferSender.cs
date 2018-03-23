﻿using SFA.DAS.Messaging.Attributes;

namespace SFA.DAS.Commitments.Events
{
    [MessageGroup("cohort_rejected_by_transfer_sender")]
    public class CohortRejectedByTransferSender
    {
        public CohortRejectedByTransferSender()
        {
        }

        public CohortRejectedByTransferSender(long accountId, long commitmentId, long transferSenderId, string userName, string userEmail)
        {
            ReceivingEmployerAccountId = accountId;
            CommitmentId = commitmentId;
            SendingEmployerAccountId = transferSenderId;
            UserName = userName;
            UserEmail = userEmail;
        }

        public long ReceivingEmployerAccountId { get; set; }
        public long CommitmentId { get; set; }
        public long SendingEmployerAccountId { get; set; }
        public string UserName { get; }
        public string UserEmail { get; }
    }
}
