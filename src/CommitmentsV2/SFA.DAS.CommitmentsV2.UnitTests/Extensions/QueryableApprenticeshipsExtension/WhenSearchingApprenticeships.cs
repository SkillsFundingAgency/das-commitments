using System.Collections.Generic;
using System.Linq;
using AutoFixture.NUnit3;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Extensions;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.CommitmentsV2.UnitTests.Extensions.QueryableApprenticeshipsExtension
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

        //todo: case insensitive
        //todo: combined first and last name
    }
}