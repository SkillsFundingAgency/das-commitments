using System;
using System.ComponentModel.DataAnnotations;
using SFA.DAS.Commitments.Api.Types.Apprenticeship.Types;
using SFA.DAS.Commitments.Api.Types.DataLock.Types;

namespace SFA.DAS.Commitments.Api.IntegrationTests.DatabaseSetup.Entities
{
    public class DbSetupDataLockStatus
    {
        public long Id { get; set; }
        public long DataLockEventId { get; set; }
        public DateTime DataLockEventDatetime { get; set; }
        [StringLength(25)]
        public string PriceEpisodeIdentifier { get; set; }
        public long ApprenticeshipId { get; set; }
        [StringLength(20)]
        public string IlrTrainingCourseCode { get; set; }
        public TrainingType IlrTrainingType { get; set; }
        public DateTime? IlrActualStartDate { get; set; }
        public DateTime? IlrEffectiveFromDate { get; set; }
        public DateTime? IlrPriceEffectiveToDate { get; set; }
        public decimal? IlrTotalCost { get; set; }
        public DataLockErrorCode ErrorCode { get; set; }
        public Status Status { get; set; }
        public TriageStatus TriageStatus { get; set; }
        public long? ApprenticeshipUpdateId { get; set; }
        public bool IsResolved { get; set; }
        public EventStatus EventStatus { get; set; }
        public bool IsExpired { get; set; }
        public DateTime? Expired { get; set; }
        //    CONSTRAINT[FK_DataLockStatus_ApprenticeshipId] FOREIGN KEY([ApprenticeshipId]) REFERENCES[Apprenticeship] ([Id]),
        //CONSTRAINT[FK_DataLockStatus_ApprenticeshipUpdateId] FOREIGN KEY([ApprenticeshipUpdateId]) REFERENCES[ApprenticeshipUpdate] ([Id])
    }
}
