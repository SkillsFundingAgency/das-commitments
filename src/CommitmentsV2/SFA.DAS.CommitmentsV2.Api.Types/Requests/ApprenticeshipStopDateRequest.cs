using System;
namespace SFA.DAS.CommitmentsV2.Api.Types.Requests;

public class ApprenticeshipStopDateRequest : SaveDataRequest
{
    public long AccountId { get; set; }
    public long CommitmentId { get; set; }
    public DateTime NewStopDate { get; set; }       
}