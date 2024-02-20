using SFA.DAS.CommitmentsV2.Extensions;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.CommitmentsV2.UnitTests.Extensions.QueryableApprenticeshipsExtensions
{
    public class WhenSearchingApprenticeships
    {
        [Test, RecursiveMoqAutoData]
        public void And_FirstName_Starts_With_SearchTerm_Then_Included(
            string searchTerm,
            List<Apprenticeship> apprenticeships)
        {
            apprenticeships[0].FirstName = searchTerm + apprenticeships[0].FirstName;
            var filter = new ApprenticeshipSearchFilters{SearchTerm = searchTerm};

            var filtered = apprenticeships.AsQueryable().Filter(filter);

            filtered.Count().Should().Be(apprenticeships.Count(apprenticeship =>
                apprenticeship.FirstName.StartsWith(searchTerm)));
        }

        [Test, RecursiveMoqAutoData]
        public void And_LastName_Starts_With_SearchTerm_Then_Included(
            string searchTerm,
            List<Apprenticeship> apprenticeships)
        {
            apprenticeships[0].LastName = searchTerm + apprenticeships[0].LastName;
            var filter = new ApprenticeshipSearchFilters{SearchTerm = searchTerm};

            var filtered = apprenticeships.AsQueryable().Filter(filter);

            filtered.Count().Should().Be(apprenticeships.Count(apprenticeship =>
                apprenticeship.LastName.StartsWith(searchTerm)));
        }

        [Test, RecursiveMoqAutoData]
        public void And_Uln_Matches_SearchTerm_Then_Included(
            long searchTerm,
            List<Apprenticeship> apprenticeships)
        {
            apprenticeships[0].Uln = searchTerm.ToString();
            var filter = new ApprenticeshipSearchFilters{SearchTerm = searchTerm.ToString()};

            var filtered = apprenticeships.AsQueryable().Filter(filter);

            filtered.Count().Should().Be(apprenticeships.Count(apprenticeship =>
                apprenticeship.Uln == searchTerm.ToString()));
        }

        [Test, RecursiveMoqAutoData]
        public void And_FirstName_Or_LastName_Matches_SearchTerm_Then_Included(
            string searchTerm,
            List<Apprenticeship> apprenticeships)
        {
            apprenticeships[0].FirstName = searchTerm;
            apprenticeships[1].LastName = searchTerm;
            var filter = new ApprenticeshipSearchFilters {SearchTerm = searchTerm};

            var filtered = apprenticeships.AsQueryable().Filter(filter);

            filtered.Count().Should().Be(apprenticeships.Count(apprenticeship =>
                apprenticeship.FirstName == searchTerm ||
                apprenticeship.LastName == searchTerm));
        }

        [Test, RecursiveMoqAutoData]
        public void And_Name_Is_Searched_With_Uln_Then_Returns_No_Results(
            string searchName,
            string searchUln,
            List<Apprenticeship> apprenticeships)
        {
            apprenticeships[0].FirstName = searchName;
            apprenticeships[0].Uln = searchUln.Substring(0, 9);
            var searchTerm = searchName + " " + searchUln;
            var filter = new ApprenticeshipSearchFilters{SearchTerm = searchTerm};

            var filtered = apprenticeships.AsQueryable().Filter(filter);

            filtered.Count().Should().Be(0);
        }

        [Test, RecursiveMoqAutoData]
        public void And_First_Name_Matches_But_Last_Name_Does_Not_Match_Search_Term_Then_Returns_Nothing(
            string searchTermFirstName,
            string searchTermLastName,
            List<Apprenticeship> apprenticeships)
        {
            apprenticeships[0].FirstName = searchTermFirstName;
            apprenticeships[0].LastName = "noMatch";
            var searchTerm = searchTermFirstName + " " + searchTermLastName;
            var filter = new ApprenticeshipSearchFilters{SearchTerm = searchTerm};

            var filtered = apprenticeships.AsQueryable().Filter(filter);

            filtered.Count().Should().Be(0);
        }

        [Test, RecursiveMoqAutoData]
        public void And_Last_Name_Matches_But_First_Name_Does_Not_Match_Search_Term_Then_Returns_Nothing(
            string searchTermFirstName,
            string searchTermLastName,
            List<Apprenticeship> apprenticeships)
        {
            apprenticeships[0].FirstName = "noMatch";
            apprenticeships[0].LastName = searchTermLastName;
            var searchTerm = searchTermFirstName + " " + searchTermLastName;
            var filter = new ApprenticeshipSearchFilters { SearchTerm = searchTerm };

            var filtered = apprenticeships.AsQueryable().Filter(filter);

            filtered.Count().Should().Be(0);
        }
    }
}