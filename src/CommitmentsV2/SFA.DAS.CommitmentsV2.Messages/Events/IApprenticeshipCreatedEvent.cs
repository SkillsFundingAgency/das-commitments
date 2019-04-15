using System;

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
        string StandardCode { get; set; }
        string ProgrammeType { get; set; }
        string FrameworkCode { get; set; }
        string PathwayCode { get; set; }
        long? TransferSenderId { get; set; }
    }

    public class PriceEpisode
    {
        public DateTime FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public decimal Cost { get; set; }
    }
}
