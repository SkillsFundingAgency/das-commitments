using AutoFixture.NUnit3;
using SFA.DAS.CommitmentsV2.Mapping.Apprenticeships;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.CommitmentsV2.UnitTests.Extensions.PriceHistoryExtensions
{
    public class WhenGettingTheLatestPrice
    {
        [Test, RecursiveMoqAutoData]
        public async Task Then_Displays_The_Cost_In_The_Valid_Date_Ranges(
            Apprenticeship source,
            decimal cost1,
            decimal cost2,
            [Frozen] Mock<ICurrentDateTime> currentDateTime,
            ApprenticeshipToApprenticeshipDetailsMapper mapper)
        {
            currentDateTime.Setup(x => x.UtcNow).Returns(DateTime.UtcNow.AddMonths(2));
            source.PriceHistory = new List<PriceHistory>{
                new PriceHistory
                {
                    ApprenticeshipId = source.Id,
                    Cost = cost1,
                    ToDate = DateTime.UtcNow.AddMonths(1),
                    FromDate = DateTime.UtcNow.AddMonths(-1)
                },
                new PriceHistory
                {
                    ApprenticeshipId = source.Id,
                    Cost = cost2,
                    
                    ToDate = DateTime.UtcNow.AddMonths(2).AddDays(1),
                    FromDate = DateTime.UtcNow.AddMonths(1).AddDays(1)
                }
            };

            var result = await mapper.Map(source);

            result.TotalAgreedPrice.Should().Be(cost2);
        }

        [Test, RecursiveMoqAutoData]
        public async Task Then_Displays_The_First_If_None_Match(
            Apprenticeship source,
            decimal cost1,
            decimal cost2,
            [Frozen] Mock<ICurrentDateTime> currentDateTime,
            ApprenticeshipToApprenticeshipDetailsMapper mapper)
        {
            currentDateTime.Setup(x => x.UtcNow).Returns(DateTime.UtcNow.AddMonths(4));
            source.PriceHistory = new List<PriceHistory>{
                new PriceHistory
                {
                    ApprenticeshipId = source.Id,
                    Cost = cost1,
                    ToDate = DateTime.UtcNow.AddMonths(2).AddDays(1),
                    FromDate = DateTime.UtcNow.AddMonths(1).AddDays(1)
                },
                new PriceHistory
                {
                    ApprenticeshipId = source.Id,
                    Cost = cost2,
                    ToDate = DateTime.UtcNow.AddMonths(1),
                    FromDate = DateTime.UtcNow.AddMonths(-1)
                }
            };

            var result = await mapper.Map(source);

            result.TotalAgreedPrice.Should().Be(cost1);
        }
    }
}
