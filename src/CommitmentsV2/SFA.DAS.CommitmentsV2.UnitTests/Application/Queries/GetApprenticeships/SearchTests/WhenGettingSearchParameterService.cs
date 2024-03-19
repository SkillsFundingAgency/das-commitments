using System;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeships.Search;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeships.Search.Services;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeships.Search.Services.Parameters;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Queries.GetApprenticeships.SearchTests
{
    public class WhenGettingSearchParameterService
    {
        [Test, MoqAutoData]
        public async Task ThenWillGetSearchHandler(
            [Frozen]Mock<IApprenticeshipSearchService<ApprenticeshipSearchParameters>> service, 
            [Frozen]Mock<IServiceProvider> serviceProvider,
            ApprenticeshipSearchParameters searchParameters,
            ApprenticeshipSearch search)
        {
            //Arrange
            var expectedResult = new ApprenticeshipSearchResult();

            serviceProvider.Setup(x =>
                    x.GetService(It.IsAny<Type>()))
                           .Returns(service.Object);

            service.Setup(x => x.Find(searchParameters))
                   .ReturnsAsync(expectedResult);

            //Act
            var result = await search.Find(searchParameters);

            //Assert
            Assert.That(result, Is.EqualTo(expectedResult));
        }
    }
}
