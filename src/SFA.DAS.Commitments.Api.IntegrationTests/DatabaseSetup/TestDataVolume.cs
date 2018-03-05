
namespace SFA.DAS.Commitments.Api.IntegrationTests.DatabaseSetup
{
    public static class TestDataVolume
    {
        //c an get datalockstatuses for provider (as we're only interested in fully approved apprenticeships), so we can remove some predicates
        public const int MinNumberOfApprenticeships = 250; //100000;
        //MinNumberOfCohorts?
        public const int MaxNumberOfApprenticeshipsInCohort = 800;
        public const double ApprenticeshipUpdatesToApprenticeshipsRatio = 0.025d;
        public const int MinNumberOfDataLockStatuses = 250; //100000; ratio instead?
        public const double SuccessDataLockStatusesToApprenticeshipsRatio = 0.98d;
        public const double ErrorDataLockStatusesToApprenticeshipsRatio = 0.025d;
        public const int MaxApprenticeshipUpdatesPerApprenticeship = 5; //todo: skew to lower?
    }
}
