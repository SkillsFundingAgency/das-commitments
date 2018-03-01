using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.Kernel;
using NUnit.Framework;
using SFA.DAS.Commitments.Api.IntegrationTests.DatabaseSetup.Entities;
using SFA.DAS.Commitments.Api.Types.Apprenticeship.Types;

namespace SFA.DAS.Commitments.Api.IntegrationTests.DatabaseSetup
{
    public static class TestData
    {
        public static async Task PopulateDatabaseWithTestData()
        {
            // get latest cohortId from database
            var latestApprenticeshipIdInDatabase = 1;
            var latestCohortIdInDatabase = 1;
            var firstNewApprenticeshipId = latestApprenticeshipIdInDatabase + 1;
            var firstNewCohortId = latestCohortIdInDatabase + 1;
            (var testApprenticeships, long lastCohortId) = TestData.GenerateApprenticeships(2, firstNewApprenticeshipId, firstNewCohortId);
            await new CommitmentsDatabase().InsertApprenticeships(testApprenticeships);

            // generate the commitments that the new apprenticeships reference
            int commitmentsToGenerate = (int)(1 + lastCohortId - firstNewCohortId);

            await TestContext.Progress.WriteLineAsync("Generating Commitments");

            var testCommitments = TestData.GenerateCommitments(commitmentsToGenerate, firstNewCohortId);
            await new CommitmentsDatabase().InsertCommitments(testCommitments);

            // (for now at least) generate non-related datalockstatuses for bulking out the table
            const long bulkApprenticeshipId = long.MaxValue, bulkApprenticeshipUpdateId = long.MaxValue;
            var testDataLockStatuses = TestData.GenerateDataLockStatuses(bulkApprenticeshipId, bulkApprenticeshipUpdateId, commitmentsToGenerate, firstNewCohortId);

            testDataLockStatuses.AddRange(TestData.GenerateDataLockStatuses(firstNewApprenticeshipId, bulkApprenticeshipUpdateId, commitmentsToGenerate, firstNewCohortId));
            await new CommitmentsDatabase().InsertDataLockStatuses(testDataLockStatuses);
        }

        public static (List<DbSetupApprenticeship>, long) GenerateApprenticeships(int apprenticeshipsToGenerate, long initialId = 1, long firstCohortId = 1, int maxCohortSize = 80)
        {
            var fixture = new Fixture();//.Customize(new IntegrationTestCustomisation());
            //fixture.Customizations.Insert(0, new RandomEnumSequenceGenerator<TableType>())
            var apprenticeships = fixture.CreateMany<DbSetupApprenticeship>(apprenticeshipsToGenerate).ToList();

            // for the first set of apprenticeships, put them in a cohort as big as maxCohortSize (given enough apprenticeships)
            // so that we have a max size cohort for testing purposes.
            // then for the other apprenticeships, give them randomly sized cohorts up to the max

            var random = new Random();
            int apprenticeshipsLeftInCohort = maxCohortSize;

            foreach (var apprenticeship in apprenticeships)
            {
                apprenticeship.Id = initialId++; //todo: required?

                if (--apprenticeshipsLeftInCohort < 0)
                {
                    apprenticeshipsLeftInCohort = random.Next(1, maxCohortSize);
                    ++firstCohortId;
                }

                apprenticeship.CommitmentId = firstCohortId;
            }

            return (apprenticeships, firstCohortId);
        }

        public static List<DbSetupCommitment> GenerateCommitments(int commitmentsToGenerate, long initialId = 1)
        {
            var fixture = new Fixture();
            var commitments = fixture.CreateMany<DbSetupCommitment>(commitmentsToGenerate).ToList();
            foreach (var commitment in commitments)
            {
                commitment.Id = initialId++;
            }
            return commitments;
        }

        public static List<DbSetupApprenticeshipUpdate> GenerateApprenticeshipUpdate(int apprenticeshipUpdatesToGenerate, long initialId = 1)
        {
            var fixture = new Fixture();
            var apprentieshipUpdates = fixture.CreateMany<DbSetupApprenticeshipUpdate>(apprenticeshipUpdatesToGenerate).ToList();
            foreach (var apprentieshipUpdate in apprentieshipUpdates)
            {
                apprentieshipUpdate.Id = initialId++;
            }
            return apprentieshipUpdates;
        }

        public static List<DbSetupDataLockStatus> GenerateDataLockStatuses(long apprenticeshipId, long apprenticeshipUpdateId, int dataLockStatusesToGenerate, long initialId = 1)
        {
            var fixture = new Fixture();
            var dataLockStatuses = fixture.CreateMany<DbSetupDataLockStatus>(dataLockStatusesToGenerate).ToList();
            foreach (var dataLockStatus in dataLockStatuses)
            {
                dataLockStatus.Id = initialId++;
                dataLockStatus.ApprenticeshipId = apprenticeshipId;
                dataLockStatus.ApprenticeshipUpdateId = apprenticeshipUpdateId;
            }
            return dataLockStatuses;
        }
    }

    //public class RandomEnumSequenceGenerator<T> : ISpecimenBuilder where T : struct
    //{
    //    private static Random _random = new Random();
    //    private Array _values;

    //    public RandomEnumSequenceGenerator()
    //    {
    //        if (!typeof(T).IsEnum)
    //        {
    //            throw new ArgumentException("T must be an enum");
    //        }
    //        _values = Enum.GetValues(typeof(T));
    //    }

    //    public object Create(object request, ISpecimenContext context)
    //    {
    //        var t = request as Type;
    //        if (t == null || t != typeof(T))
    //            return new NoSpecimen();

    //        var index = _random.Next(0, _values.Length - 1);
    //        return _values.GetValue(index);
    //    }
    //}
}
