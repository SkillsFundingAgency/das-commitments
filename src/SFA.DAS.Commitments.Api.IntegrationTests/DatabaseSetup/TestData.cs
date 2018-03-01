using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.Kernel;
using SFA.DAS.Commitments.Api.IntegrationTests.DatabaseSetup.Entities;
using SFA.DAS.Commitments.Api.Types.Apprenticeship.Types;

namespace SFA.DAS.Commitments.Api.IntegrationTests.DatabaseSetup
{
    public class TestData
    {
        public (List<DbSetupApprenticeship>, long) GenerateApprenticeships(int apprenticeshipsToGenerate, long initialId = 1, long firstCohortId = 1, int maxCohortSize = 80)
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

        public List<DbSetupCommitment> GenerateCommitments(int commitmentsToGenerate, long initialId = 1)
        {
            var fixture = new Fixture();
            var commitments = fixture.CreateMany<DbSetupCommitment>(commitmentsToGenerate).ToList();
            foreach (var commitment in commitments)
            {
                commitment.Id = initialId++;
            }
            return commitments;
        }
    }

    public class RandomEnumSequenceGenerator<T> : ISpecimenBuilder where T : struct
    {
        private static Random _random = new Random();
        private Array _values;

        public RandomEnumSequenceGenerator()
        {
            if (!typeof(T).IsEnum)
            {
                throw new ArgumentException("T must be an enum");
            }
            _values = Enum.GetValues(typeof(T));
        }

        public object Create(object request, ISpecimenContext context)
        {
            var t = request as Type;
            if (t == null || t != typeof(T))
                return new NoSpecimen();

            var index = _random.Next(0, _values.Length - 1);
            return _values.GetValue(index);
        }
    }
}
