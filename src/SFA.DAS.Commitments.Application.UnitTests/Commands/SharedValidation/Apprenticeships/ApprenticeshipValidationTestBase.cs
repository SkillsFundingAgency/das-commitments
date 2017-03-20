using System;
using Moq;
using NUnit.Framework;
using SFA.DAS.Commitments.Api.Types;
using SFA.DAS.Commitments.Api.Types.Apprenticeship;
using SFA.DAS.Commitments.Application.Commands;
using SFA.DAS.Commitments.Domain.Interfaces;

namespace SFA.DAS.Commitments.Application.UnitTests.Commands.SharedValidation.Apprenticeships
{
    public abstract class ApprenticeshipValidationTestBase
    {
        protected ApprenticeshipValidator Validator;
        protected Apprenticeship ExampleValidApprenticeship;
        protected Mock<ICurrentDateTime> MockCurrentDateTime;

        [SetUp]
        public void BaseSetup()
        {
            MockCurrentDateTime = new Mock<ICurrentDateTime>();
            MockCurrentDateTime.SetupGet(x => x.Now).Returns(new DateTime(2017, 6, 10));

            Validator = new ApprenticeshipValidator(MockCurrentDateTime.Object);

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
