using AutoFixture.Kernel;
using SFA.DAS.CommitmentsV2.Models;

namespace SFA.DAS.CommitmentsV2.UnitTests
{
    public class ModelSpecimenBuilder : ISpecimenBuilder
    {
        public object Create(object request,
            ISpecimenContext context)
        {
            var pi = request as Type;

            if (pi == null)
            {
                return new NoSpecimen();
            }

            if (pi == typeof(ApprenticeshipBase) || pi.Name == nameof(Apprenticeship))
            {
                return new Apprenticeship();
            }

            if (pi == typeof(ApprenticeshipUpdate))
            {
                return new ApprenticeshipUpdate();
            }

            return new NoSpecimen();
        }
    }
}
