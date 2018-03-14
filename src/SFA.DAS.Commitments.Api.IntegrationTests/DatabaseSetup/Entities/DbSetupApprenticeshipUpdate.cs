using System;
using System.ComponentModel.DataAnnotations;
using SFA.DAS.Commitments.Api.Types.Apprenticeship.Types;

namespace SFA.DAS.Commitments.Api.IntegrationTests.DatabaseSetup.Entities
{
    public class DbSetupApprenticeshipUpdate
    {
        public long Id { get; set; }
        public long ApprenticeshipId { get; set; }
        public Originator Originator { get; set; }
        public ApprenticeshipUpdateStatus Status { get; set; }
        [StringLength(100)]
        public string FirstName { get; set; }
        [StringLength(100)]
        public string LastName { get; set; }
        public TrainingType? TrainingType { get; set; }
        [StringLength(20)]
        public string TrainingCode { get; set; }
        [StringLength(126)]
        public string TrainingName { get; set; }
        public decimal? Cost { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public DateTime? CreatedOn { get; set; }
        public UpdateOrigin? UpdateOrigin { get; set; }
        public DateTime? EffectiveFromDate { get; set; }
        public DateTime? EffectiveToDate { get; set; }
    }
}
