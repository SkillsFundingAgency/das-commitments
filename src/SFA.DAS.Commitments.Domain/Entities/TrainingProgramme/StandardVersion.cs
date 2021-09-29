using System;

namespace SFA.DAS.Commitments.Domain.Entities.TrainingProgramme
{
    public class StandardVersion
    {
        public StandardVersion()
        {
        }

        public string StandardUId { get; set; }
        public long LarsCode { get; set; }
        public string IFateReferenceNumber { get; set; }
        public string Version { get; set; }
        public string Title { get; set; }
        public int Level { get; set; }
        public int Duration { get; set; }
        public int MaxFunding { get; set; }
        public DateTime? EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }
        public int VersionMajor { get; set; }
        public int VersionMinor { get; set; }
        public DateTime? VersionEarliestStartDate { get; set; }
        public DateTime? VersionLatestStartDate { get; set; }
    }
}