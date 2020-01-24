using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeships;
using SFA.DAS.CommitmentsV2.Mapping.Apprenticeships;
using SFA.DAS.CommitmentsV2.Mapping.ResponseMappers;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.CommitmentsV2.UnitTests.Mapping.ResponseMappers
{
    public class GetApprenticeshipsResponseMapperTests
    {
        [Test, RecursiveMoqAutoData]
        public async Task Then_Maps_GetApprenticeshipsQueryResultToGetApprenticeshipsResponse(
            GetApprenticeshipsQueryResult source,
            GetApprenticeshipsResponseMapper mapper)
        {
            var result = await mapper.Map(source);

            result.Apprenticeships.First().Should().BeEquivalentTo(source.Apprenticeships.First());
            result.TotalApprenticeshipsFound.Should().Be(source.TotalApprenticeshipsFound);
            result.TotalApprenticeshipsWithAlertsFound.Should().Be(source.TotalApprenticeshipsWithAlertsFound);
        }
    }
}
