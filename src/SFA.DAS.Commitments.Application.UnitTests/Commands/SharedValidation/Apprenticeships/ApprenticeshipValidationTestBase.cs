using System;
using Moq;
using NUnit.Framework;
using SFA.DAS.Commitments.Application.Commands;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Interfaces;
using SFA.DAS.Learners.Validators;

namespace SFA.DAS.Commitments.Application.UnitTests.Commands.SharedValidation.Apprenticeships
{
    public abstract class ApprenticeshipValidationTestBase
    {
        protected ApprenticeshipValidator Validator;
        protected Apprenticeship ExampleValidApprenticeship;
        protected Mock<ICurrentDateTime> MockCurrentDateTime;
        protected  Mock<IUlnValidator> MockUlnValidator;

        [SetUp]
        public void BaseSetup()
        {
            MockCurrentDateTime = new Mock<ICurrentDateTime>();
            MockCurrentDateTime.SetupGet(x => x.Now).Returns(new DateTime(2017, 6, 10));

            MockUlnValidator = new Mock<IUlnValidator>();

            Validator = new ApprenticeshipValidator(MockCurrentDateTime.Object, MockUlnValidator.Object);

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
