using System;
using NUnit.Framework;
using SFA.DAS.Commitments.Api.Types;
using SFA.DAS.Commitments.Application.Commands;

namespace SFA.DAS.Commitments.Application.UnitTests.Commands.SharedValidation.Apprenticeships
{
    public abstract class ApprenticeshipValidationTestBase
    {
        protected ApprenticeshipValidator Validator;
        protected Apprenticeship ExampleValidApprenticeship;

        [SetUp]
        public void BaseSetup()
        {
            Validator = new ApprenticeshipValidator();

            ExampleValidApprenticeship = new Apprenticeship
            {
                FirstName = "Bob",
                LastName = "Smith",
                NINumber = ApprenticeshipTestDataHelper.CreateValidNino(),
                ULN = ApprenticeshipTestDataHelper.CreateValidULN(),
                ProviderRef = "Provider ref",
                EmployerRef = null,
                StartDate = DateTime.Now.AddYears(5),
                EndDate = DateTime.Now.AddYears(7)
            };
        }
    }
}
