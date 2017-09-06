using System;
using System.Collections.Generic;
using System.Linq;

using FluentAssertions;

using NUnit.Framework;
using SFA.DAS.Commitments.Api.Orchestrators.Mappers;
using SFA.DAS.Commitments.Api.Types.Apprenticeship;
using SFA.DAS.Commitments.Api.Types.Apprenticeship.Types;

namespace SFA.DAS.Commitments.Api.UnitTests.Mapping.Service.ApprenticeshipFilterService
{
    [TestFixture]
    public class WhenSearchingWithAKeyword
    {
        private Api.Orchestrators.Mappers.ApprenticeshipFilterService _sut;

        private List<Apprenticeship> _apprenticeships;

        [SetUp]
        public void SetUp()
        {
            _sut = new Api.Orchestrators.Mappers.ApprenticeshipFilterService(new FacetMapper(new StubCurrentDateTime()));

            _apprenticeships = new List<Apprenticeship>
                                   {
                                       new Apprenticeship
                                           {
                                               Id = 006,
                                               FirstName = "Chris",
                                               LastName = "Foster",
                                               PaymentStatus = PaymentStatus.Active,
                                               StartDate = DateTime.Now.AddDays(-60),
                                               ULN = "1112223330"
                                           },
                                       new Apprenticeship
                                           {
                                               Id = 007,
                                               FirstName = "Chris",
                                               LastName = "Froberg",
                                               PaymentStatus = PaymentStatus.Active,
                                               StartDate = DateTime.Now.AddMonths(2),
                                               DataLockCourseTriaged = true,
                                               ULN = "1112223331"
                                           },
                                       new Apprenticeship
                                           {
                                               Id = 008,
                                               FirstName = "Mathias",
                                               LastName = "Hayashi",
                                               PaymentStatus = PaymentStatus.Active,
                                               StartDate = DateTime.Now.AddMonths(2),
                                               DataLockCourseTriaged = true,
                                               ULN = "1112223332"
                                           }
                                   };
        }

        [TestCase("Chris", 2, Description = "Serach firstname only")]
        [TestCase("Hayashi", 1, Description = "Serach lastname only")]
        [TestCase("Foster", 1, Description = "Serach lastname only")]
        [TestCase("HAYAshI", 1, Description = "Serach lastname only - Should ignore case")]
        [TestCase("Chris Foster", 1, Description = "Serach first and lastname")]
        [TestCase("Foster Chris", 0, Description = "Serach first and lastname - should care about order")]
        [TestCase("YAsh", 1, Description = "Serach partial lastname")]
        [TestCase("hias Haya", 1, Description = "Serach partial first and lastname")]
        [TestCase("", 3, Description = "Serach with empty string should return all apprenticeships")]
        public void SearchForProvider(string searchTerm, int expectedResultCount)
        {
            var result = _sut.Search(_apprenticeships, searchTerm, Originator.Provider);
            result.Count.Should().Be(expectedResultCount);
        }

        [TestCase("1112223332", 1, "Hayashi", Description = "Should find apprenticeship by ULN")]
        [TestCase("11122233320", 0, "", Description = "Should not find apprenticeship by ULN if search term is more than 10 digits")]
        [TestCase("111222333", 0, "", Description = "Should not find apprenticeship by ULN if search term is less than 10 digits")]
        [TestCase("2112223332", 0, "", Description = "Should not find apprenticeship by ULN if no found")]
        public void SearchUlnForProvider(string searchTerm, int expectedResultCount, string expectedLastName)
        {
            var result = _sut.Search(_apprenticeships, searchTerm, Originator.Provider);
            result.Count.Should().Be(expectedResultCount);
            if (expectedResultCount > 0)
            {
                result.FirstOrDefault().ULN.Should().Be(searchTerm);
                result.FirstOrDefault().LastName.Should().Be(expectedLastName);
            }
        }

        [TestCase("Chris", 2, Description = "Serach firstname only")]
        [TestCase("Hayashi", 1, Description = "Serach lastname only")]
        [TestCase("Foster", 1, Description = "Serach lastname only")]
        [TestCase("HAYAshI", 1, Description = "Serach lastname only - Should ignore case")]
        [TestCase("Chris Foster", 1, Description = "Serach first and lastname")]
        [TestCase("Foster Chris", 0, Description = "Serach first and lastname - should care about order")]
        [TestCase("YAsh", 1, Description = "Serach partial lastname")]
        [TestCase("hias Haya", 1, Description = "Serach partial first and lastname")]
        [TestCase("", 3, Description = "Serach with empty string should return all apprenticeships")]
        public void SearchForEmployer(string searchTerm, int expectedResultCount)
        {
            var result = _sut.Search(_apprenticeships, searchTerm, Originator.Employer);
            result.Count.Should().Be(expectedResultCount);
        }

        [TestCase("1112223332", 0, "Hayashi", Description = "Should not find apprenticeship by ULN for employer")]
        [TestCase("11122233320", 0, "", Description = "Should not find apprenticeship by ULN for employer")]
        [TestCase("111222333", 0, "", Description = "Should not find apprenticeship by ULN for employer")]
        [TestCase("2112223332", 0, "", Description = "Should not find apprenticeship by ULN for employer")]
        public void SearchUlnForEmployer(string searchTerm, int expectedResultCount, string expectedLastName)
        {
            var result = _sut.Search(_apprenticeships, searchTerm, Originator.Employer);
            result.Count.Should().Be(expectedResultCount);
            if (expectedResultCount > 0)
            {
                result.FirstOrDefault().ULN.Should().Be(searchTerm);
                result.FirstOrDefault().LastName.Should().Be(expectedLastName);
            }
        }
    }
}
