using MediatR;
using SFA.DAS.CommitmentsV2.Types;
using System;

namespace SFA.DAS.CommitmentsV2.Application.Commands.UpdateApprenticeshipStopDate
{
    public class UpdateApprenticeshipStopDateCommand : IRequest
    {
        public long AccountId { get; }
       //public long CommitmentId { get; } // TO DO :check and remove
        public long ApprenticeshipId { get; }        
        public DateTime StopDate { get; }
        public UserInfo UserInfo { get; }

        public UpdateApprenticeshipStopDateCommand(long accountId, long apprenticeshipId, DateTime stopDate,  UserInfo userInfo)
        {
            AccountId = accountId;
            ApprenticeshipId = apprenticeshipId;
            StopDate = stopDate;
            UserInfo = userInfo;
        }
        
        // TO DO : we dont need this : we  need partycheck .. need to check 
        //public Caller Caller { get; set; }
    }

    //public class Caller
    //{
    //    public Caller() { }

    //    public Caller(long id, CallerType type)
    //    {
    //        Id = id;
    //        CallerType = type;
    //    }

    //    public long Id { get; set; }
    //    public CallerType CallerType { get; set; }
    //}


    //public enum CallerType
    //{
    //    Employer = 0,
    //    Provider,
    //    TransferSender,
    //    TransferReceiver,
    //    Support
    //}
}
