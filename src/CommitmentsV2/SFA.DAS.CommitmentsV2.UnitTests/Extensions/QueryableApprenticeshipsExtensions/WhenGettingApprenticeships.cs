using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeships.Search.Services.Parameters;
using SFA.DAS.CommitmentsV2.Extensions;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.CommitmentsV2.UnitTests.Extensions.QueryableApprenticeshipsExtensions
{
    public class WhenGettingApprenticeships
    {

        [Test, RecursiveMoqAutoData]
        public void Then_Returns_Provider_Apprenticeships(
            ApprenticeshipSearchParameters searchParameters,
            List<Apprenticeship> apprenticeships)
        {
            searchParameters.PageNumber = 0;
            searchParameters.PageItemCount = 0;
            searchParameters.Filters = new ApprenticeshipSearchFilters();
            searchParameters.EmployerAccountId = null;

            apprenticeships[0].Cohort.ProviderId = searchParameters.ProviderId ?? 0;
            apprenticeships[1].Cohort.ProviderId = searchParameters.ProviderId ?? 0;

            var expectedApprenticeships =
                apprenticeships.Where(app => app.Cohort.ProviderId == searchParameters.ProviderId);

            var result = apprenticeships.AsQueryable().WithProviderOrEmployerId(searchParameters);

            result.Count()
                .Should().Be(apprenticeships
                    .Count(apprenticeship => apprenticeship.Cohort.ProviderId == searchParameters.ProviderId));

            result.Should().BeEquivalentTo(expectedApprenticeships);
        }

        [Test, RecursiveMoqAutoData]
        public void Then_Returns_Employer_Apprenticeships(
            ApprenticeshipSearchParameters searchParameters,
            List<Apprenticeship> apprenticeships)
        {
            searchParameters.PageNumber = 0;
            searchParameters.PageItemCount = 0;
            searchParameters.Filters = new ApprenticeshipSearchFilters();
            searchParameters.ProviderId = null;

            apprenticeships[0].Cohort.EmployerAccountId = searchParameters.EmployerAccountId.Value;
            apprenticeships[1].Cohort.EmployerAccountId = searchParameters.EmployerAccountId.Value;

            var expectedApprenticeships =
                apprenticeships.Where(app => app.Cohort.EmployerAccountId == searchParameters.EmployerAccountId);

            var result = apprenticeships.AsQueryable().WithProviderOrEmployerId(searchParameters);

            result.Count()
                .Should().Be(apprenticeships
                    .Count(apprenticeship => apprenticeship.Cohort.EmployerAccountId == searchParameters.EmployerAccountId));

            result.Should().BeEquivalentTo(expectedApprenticeships);
        }

        [Test, RecursiveMoqAutoData]
        public void Then_Returns_All_Apprenticeships_If_No_Provider_Or_Employer_Id_Found(
            ApprenticeshipSearchParameters searchParameters,
            List<Apprenticeship> apprenticeships)
        {
            searchParameters.PageNumber = 0;
            searchParameters.PageItemCount = 0;
            searchParameters.Filters = new ApprenticeshipSearchFilters();
            searchParameters.ProviderId = null;
            searchParameters.EmployerAccountId = null;

            var result = apprenticeships.AsQueryable().WithProviderOrEmployerId(searchParameters);

            result.Should().BeEquivalentTo(apprenticeships);
        }
    }
}
