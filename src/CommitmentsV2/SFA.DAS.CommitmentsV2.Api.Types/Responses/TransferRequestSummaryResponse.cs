﻿using SFA.DAS.CommitmentsV2.Types;
using System;
using System.Collections.Generic;

namespace SFA.DAS.CommitmentsV2.Api.Types.Responses
{
    public class GetTransferRequestSummaryResponse
    {
        public IEnumerable<TransferRequestSummaryResponse> TransferRequestSummaryResponse { get; set; }
    }

    public class TransferRequestSummaryResponse
    {
        public string HashedTransferRequestId { get; set; }
        public string HashedReceivingEmployerAccountId { get; set; }
        public string CohortReference { get; set; }
        public string HashedSendingEmployerAccountId { get; set; }
        public long CommitmentId { get; set; }
        public decimal TransferCost { get; set; }
        public TransferApprovalStatus Status { get; set; }
        public string ApprovedOrRejectedByUserName { get; set; }
        public string ApprovedOrRejectedByUserEmail { get; set; }
        public DateTime? ApprovedOrRejectedOn { get; set; }
        public TransferType TransferType { get; set; }
        public DateTime CreatedOn { get; set; }
        public int FundingCap { get; set; }
    }
}
