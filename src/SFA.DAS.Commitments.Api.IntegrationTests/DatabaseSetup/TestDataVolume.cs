
namespace SFA.DAS.Commitments.Api.IntegrationTests.DatabaseSetup
{
    public static class TestDataVolume
    {
        //can get datalockstatuses for provider (as we're only interested in fully approved apprenticeships), so we can remove some predicates
        public const int MinNumberOfApprenticeships = 250000;
        //MinNumberOfCohorts?
        public const int MaxNumberOfApprenticeshipsInCohort = 80;
        public const double ApprenticeshipUpdatesToApprenticeshipsRatio = 0.025d;
        public const double SuccessDataLockStatusesToApprenticeshipsRatio = 0.98d;
        public const double ErrorDataLockStatusesToApprenticeshipsRatio = 0.025d;
        public const int MaxApprenticeshipUpdatesPerApprenticeship = 5; //todo: skew to lower?
        //todo: change to 0 - 3
        // some apprenticeships don't have datalockstatuses (yet)
        // you get 1 per price episode identifier, and per academic year, so currently there are about 1 per apprenticeship
        // but in subsequent years (with courses lasting say 3 years), the average will rise to about 3
        // we should probably define percentages here, and not use the 2 ratios above for the volume, just ratio of success to error
        public const int MaxDataLockStatusesPerApprenticeship = 5; // status & error versions?
    }
}
