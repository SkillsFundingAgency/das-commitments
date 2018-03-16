
using SFA.DAS.Commitments.Api.Types.DataLock.Types;
using EventStatusProbability = SFA.DAS.Commitments.Api.IntegrationTests.DatabaseSetup.ProbabilityDistribution<SFA.DAS.Commitments.Api.Types.DataLock.Types.EventStatus>;
using DataLockStatusCountProbability = SFA.DAS.Commitments.Api.IntegrationTests.DatabaseSetup.ProbabilityDistribution<int>;

namespace SFA.DAS.Commitments.Api.IntegrationTests.DatabaseSetup
{
    public static class TestDataVolume
    {
        //can get datalockstatuses for provider (as we're only interested in fully approved apprenticeships), so we can remove some predicates
        public const int MinNumberOfApprenticeships = 250000;
        //MinNumberOfCohorts?
        public const int MaxNumberOfApprenticeshipsInCohort = 80;

        #region ApprenticeshipUpdate

        public const double ApprenticeshipUpdatesToApprenticeshipsRatio = 0.025d;
        public const int MaxApprenticeshipUpdatesPerApprenticeship = 5; //todo: skew to lower?

        #endregion ApprenticeshipUpdate

        #region DataLockStatus

        public const double ErrorDataLockStatusProbability = 0.025d;

        // some apprenticeships don't have datalockstatuses (yet)
        // you get 1 per price episode identifier, and per academic year, so currently there are about 1 per apprenticeship
        // but in subsequent years (with courses lasting say 3 years), the average will rise to about 3

        public static DataLockStatusCountProbability DataLockStatusesPerApprenticeshipProbability = new DataLockStatusCountProbability(
            new[]
            {
                new DataLockStatusCountProbability.BoundaryValue(   50_000, () => 0),
                new DataLockStatusCountProbability.BoundaryValue(  900_000, () => 1),
                new DataLockStatusCountProbability.BoundaryValue(  950_000, () => 2),
                new DataLockStatusCountProbability.BoundaryValue(  980_000, () => 3),
                new DataLockStatusCountProbability.BoundaryValue(  995_000, () => 4),
                new DataLockStatusCountProbability.BoundaryValue(1_000_000, () => 5)
            });

        public static EventStatusProbability DataLockStatusEventStatusProbability = new EventStatusProbability(
            new []
            {
                new EventStatusProbability.BoundaryValue(10, () => EventStatus.New),
                new EventStatusProbability.BoundaryValue(20, () => EventStatus.Updated),
                new EventStatusProbability.BoundaryValue(100, () => EventStatus.Removed) // majority are removed
            });

        #endregion DataLockStatus
    }
}
