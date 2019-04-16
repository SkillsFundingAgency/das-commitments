using System;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Messages.Events
{
    public interface IApprenticeshipCreatedEvent
    {
        long ApprenticeshipId { get; set; }
        DateTime CreatedOn { get; set; }
        string Uln { get; set; }
        long ProviderId { get; set; }
        long AccountId { get; set; }
        string AccountLegalEntityPublicHashedId { get; set; }
        string LegalEntityName { get; set; }
        DateTime StartDate { get; set; }
        DateTime EndDate { get; set; }
        PriceEpisode[] PriceEpisodes { get; set; }
        TrainingType TrainingType { get; set; }
        string TrainingCode { get; set; }
        long? TransferSenderId { get; set; }
    }
}
