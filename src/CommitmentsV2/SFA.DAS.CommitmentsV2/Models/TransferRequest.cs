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
                throw new InvalidOperationException($"The TransferRequest {Id} is not in a Pending State and has a current status of {Status}");
            }

            StartTrackingSession(UserAction.ApproveTransferRequest, Party.TransferSender, Cohort.EmployerAccountId, Cohort.ProviderId, userInfo);
            ChangeTrackingSession.TrackUpdate(this);
            TransferApprovalActionedByEmployerName = userInfo.UserDisplayName;
            TransferApprovalActionedByEmployerEmail = userInfo.UserEmail;
            Status = TransferApprovalStatus.Approved;
            TransferApprovalActionedOn = now;
            ChangeTrackingSession.CompleteTrackingSession();

            Publish(() => new TransferRequestApprovedEvent(Id, Cohort.Id, now, userInfo, Cohort.PledgeApplicationId));
        }

        public void Reject(UserInfo userInfo, DateTime rejectedOn)
        {
            if (Status != TransferApprovalStatus.Pending)
            {
                throw new InvalidOperationException($"The TransferRequest {Id} is not in a Pending State and has a current status of {Status}");
            }

            StartTrackingSession(UserAction.RejectTransferRequest, Party.TransferSender, Cohort.EmployerAccountId, Cohort.ProviderId, userInfo);
            ChangeTrackingSession.TrackUpdate(this);
            TransferApprovalActionedByEmployerName = userInfo.UserDisplayName;
            TransferApprovalActionedByEmployerEmail = userInfo.UserEmail;
            Status = TransferApprovalStatus.Rejected;
            TransferApprovalActionedOn = rejectedOn;
            ChangeTrackingSession.CompleteTrackingSession();

            Publish( () => new TransferRequestRejectedEvent(Id, Cohort.Id, rejectedOn, userInfo));
        }
    }
}
