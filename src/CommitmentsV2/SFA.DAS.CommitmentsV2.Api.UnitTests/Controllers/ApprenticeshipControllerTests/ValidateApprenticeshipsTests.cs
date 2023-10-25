using System;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Api.Controllers;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeshipsValidate;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.CommitmentsV2.Api.UnitTests.Controllers.ApprenticeshipControllerTests
{

    public class ValidateApprenticeshipsTests
    {
        [Test, MoqAutoData]
        public async Task ValidateApprenticeships_ReturnsQueryResults(
            [Frozen] Mock<IMediator> mediatorMock,
            [Greedy] ApprenticeshipController sut,
            GetApprenticeshipsValidateQueryResult expectedResult,
            string lastName,
            string firstName,
            DateTime dateOfBirth)
        {
            mediatorMock.Setup(m => m.Send(It.Is<GetApprenticeshipsValidateQuery>(q => q.FirstName == firstName && q.LastName == lastName && q.DateOfBirth == dateOfBirth), It.IsAny<CancellationToken>())).ReturnsAsync(expectedResult);

            var actual = await sut.ValidateApprenticeship(firstName, lastName, dateOfBirth);

            actual.As<OkObjectResult>().Value.As<GetApprenticeshipsValidateQueryResult>().Should().NotBeNull();
            actual.As<OkObjectResult>().Value.As<GetApprenticeshipsValidateQueryResult>().Apprenticeships.Should().BeSameAs(expectedResult.Apprenticeships);
        }
    }
}