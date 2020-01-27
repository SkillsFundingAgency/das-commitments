using System;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.CommitmentsV2.Models.Interfaces;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Models
{
    public class TransferRequest : Aggregate, ITrackableEntity
    {
        public TransferRequest()
        {
        }

        public long Id { get; set; }
        public long CommitmentId { get; set; }
        public string TrainingCourses { get; set; }
        public decimal Cost { get; set; }
        public TransferApprovalStatus Status { get; set; }
        public string TransferApprovalActionedByEmployerName { get; set; }
        public string TransferApprovalActionedByEmployerEmail { get; set; }
        public DateTime? TransferApprovalActionedOn { get; set; }
        public DateTime CreatedOn { get; set; }
        public decimal? FundingCap { get; set; }
        public virtual Cohort Cohort { get; set; }


        public void Approve(UserInfo userInfo, DateTime now)
        {
            if (Status != TransferApprovalStatus.Pending)
            {
                throw new InvalidOperationException($"The TransferRequest is not in a Pending State and has already been approved or rejected");
            }

            StartTrackingSession(UserAction.ApproveTransferRequest, Party.TransferSender, Cohort.EmployerAccountId, Cohort.ProviderId.Value, userInfo);
            ChangeTrackingSession.TrackUpdate(this);
            TransferApprovalActionedByEmployerName = userInfo.UserDisplayName;
            TransferApprovalActionedByEmployerEmail = userInfo.UserEmail;
            Status = TransferApprovalStatus.Approved;
            TransferApprovalActionedOn = now;
            ChangeTrackingSession.CompleteTrackingSession();

            Publish(() => new TransferRequestApprovedEvent(Id, Cohort.Id, now, userInfo));
        }
    }
}
