
using SFA.DAS.Commitments.Api.IntegrationTests.Helpers.Probability;
using SFA.DAS.Commitments.Api.Types.DataLock.Types;
using EventStatusProbability = SFA.DAS.Commitments.Api.IntegrationTests.Helpers.Probability.ProbabilityDistribution<SFA.DAS.Commitments.Api.Types.DataLock.Types.EventStatus>;
using EventStatusBoundary = SFA.DAS.Commitments.Api.IntegrationTests.Helpers.Probability.BoundaryValue<SFA.DAS.Commitments.Api.Types.DataLock.Types.EventStatus>;
//using ApprenticeshipUpdateCountProbability = SFA.DAS.Commitments.Api.IntegrationTests.Helpers.ProbabilityDistribution<int>;

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

        //todo:
        //public static ApprenticeshipUpdateCountProbability ApprenticeshipUpdatesPerApprenticeshipProbability = new ApprenticeshipUpdateCountProbability(
        //new []
        //{
        //    new ApprenticeshipUpdateCountProbability.BoundaryValue(), 
        //});

        #endregion ApprenticeshipUpdate

        #region DataLockStatus

        public const double ErrorDataLockStatusProbability = 0.025d;

        // some apprenticeships don't have datalockstatuses (yet)
        // you get 1 per price episode identifier, and per academic year, so currently there are about 1 per apprenticeship
        // but in subsequent years (with courses lasting say 3 years), the average will rise to about 3

        public static ProbabilityDistribution<int> DataLockStatusesPerApprenticeshipProbability = new ProbabilityDistribution<int>(
            new[]
            {
                new BoundaryValue<int>(   50_000, () => 0),
                new BoundaryValue<int>(  900_000, () => 1),
                new BoundaryValue<int>(  950_000, () => 2),
                new BoundaryValue<int>(  980_000, () => 3),
                new BoundaryValue<int>(  995_000, () => 4),
                new BoundaryValue<int>(1_000_000, () => 5)
            });

        public static EventStatusProbability DataLockStatusEventStatusProbability = new EventStatusProbability(
            new []
            {
                new EventStatusBoundary(10, () => EventStatus.New),
                new EventStatusBoundary(20, () => EventStatus.Updated),
                new EventStatusBoundary(100, () => EventStatus.Removed) // majority are removed
            });

        #endregion DataLockStatus

        #region PriceHistory

        public static ProbabilityDistribution<int> PriceHistoryPerApprenticeshipProbability = new ProbabilityDistribution<int>(
            new[]
            {   //todo:
                new BoundaryValue<int>(10, () => 0),
                new BoundaryValue<int>(90, () => 1),
                new BoundaryValue<int>(100, () => 2)
            });

        #endregion PriceHistory
    }
}
