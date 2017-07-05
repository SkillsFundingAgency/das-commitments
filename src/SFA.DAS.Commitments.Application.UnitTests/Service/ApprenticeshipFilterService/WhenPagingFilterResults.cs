using System;
using System.Collections.Generic;
using System.Linq;

using FluentAssertions;
using NUnit.Framework;

using SFA.DAS.Commitments.Api.Types.Apprenticeship;
using SFA.DAS.Commitments.Api.Types.Apprenticeship.Types;


namespace SFA.DAS.Commitments.Application.UnitTests.Service.ApprenticeshipFilterService
{
    using SFA.DAS.Commitments.Application.Services;

    [TestFixture]
    public class WhenPagingFilterResults
    {
        private ApprenticeshipFilterService _sut;

        [SetUp]
        public void SetUp()
        {
            _sut = new ApprenticeshipFilterService(new FacetMapper());
        }

        [Test]
        public void ShouldReturnFirstPageOfApprenticeships()
        {
            const int RequestedPageNumber = 1;
            const int RequestedPageSize = 4;

            var apprenticeships = CreatePaginiationApprenticeships(20);
            var query = new ApprenticeshipSearchQuery { PageNumber = RequestedPageNumber, PageSize = RequestedPageSize };

            var result = _sut.Filter(apprenticeships, query, Originator.Provider);

            result.PageOfResults.Count.Should().Be(RequestedPageSize);
            result.PageOfResults.Select(x => x.Id).ShouldAllBeEquivalentTo(new List<int> { 1, 2, 3, 4 });
        }

        [Test]
        public void ShouldReturnSecondPageOfApprenticeships()
        {
            const int RequestedPageNumber = 2;
            const int RequestedPageSize = 4;

            var apprenticeships = CreatePaginiationApprenticeships(20);
            var query = new ApprenticeshipSearchQuery { PageNumber = RequestedPageNumber, PageSize = RequestedPageSize };

            var result = _sut.Filter(apprenticeships, query, Originator.Provider);

            result.PageOfResults.Count.Should().Be(RequestedPageSize);
            result.PageOfResults.Select(x => x.Id).ShouldAllBeEquivalentTo(new List<int> { 5, 6, 7, 8 });
        }

        [Test]
        public void ShouldReturnFullPageForLastPage()
        {
            const int RequestedPageNumber = 5;
            const int RequestedPageSize = 4;

            var apprenticeships = CreatePaginiationApprenticeships(20);
            var query = new ApprenticeshipSearchQuery { PageNumber = RequestedPageNumber, PageSize = RequestedPageSize };

            var result = _sut.Filter(apprenticeships, query, Originator.Provider);

            result.PageOfResults.Count.Should().Be(RequestedPageSize);
            result.PageOfResults.Select(x => x.Id).ShouldAllBeEquivalentTo(new List<int> { 17, 18, 19, 20 });
        }

        [Test]
        public void ShouldReturnPartialPageForLastPage()
        {
            const int RequestedPageNumber = 5;
            const int RequestedPageSize = 4;

            var apprenticeships = CreatePaginiationApprenticeships(18);
            var query = new ApprenticeshipSearchQuery { PageNumber = RequestedPageNumber, PageSize = RequestedPageSize };

            var result = _sut.Filter(apprenticeships, query, Originator.Provider);

            result.PageOfResults.Count.Should().Be(RequestedPageSize - 2);
            result.PageOfResults.Select(x => x.Id).ShouldAllBeEquivalentTo(new List<int> { 17, 18 });
        }

        [TestCase(1, 100, 10, 1, Description = "Returns first page number if first page number passed in")]
        [TestCase(14, 100, 10, 10, Description = "Returns last page number if page is not within range of total pages")]
        [TestCase(10, 100, 10, 10, Description = "Returns page number if page is not within range of total pages")]
        [TestCase(0, 100, 10, 1, Description = "Returns first page if page is not set (0)")]
        [TestCase(-3, 100, 10, 1, Description = "Returns first page if page less than zero")]
        public void ShouldReturnThePageNumber(int requestedPageNumber, int totalApprenticeships, int requestedPageSize, int expectedPageNumber)
        {
            var apprenticeships = CreatePaginiationApprenticeships(totalApprenticeships);

            var query = new ApprenticeshipSearchQuery { PageNumber = requestedPageNumber, PageSize = requestedPageSize };

            var result = _sut.Filter(apprenticeships, query, Originator.Provider);

            result.PageNumber.Should().Be(expectedPageNumber);
        }

        [TestCase(5, 5, Description = "Returns page size from query if > 0")]
        [TestCase(0, 25, Description = "Defaults page size to 25 if the page size is not set (0)")]
        [TestCase(-34, 25, Description = "Defaults page size to 25 if the page size is set to negative number")]
        public void ShouldReturnThePageSize(int requestedPageSize, int expectedPageSize)
        {
            var apprenticeships = CreatePaginiationApprenticeships(20);

            var query = new ApprenticeshipSearchQuery { PageNumber = 1, PageSize = requestedPageSize };

            var result = _sut.Filter(apprenticeships, query, Originator.Provider);

            result.PageSize.Should().Be(expectedPageSize);
        }

        private static IList<Apprenticeship> CreatePaginiationApprenticeships(int count)
        {
            var apprenticeships = new List<Apprenticeship>(count);

            for (int i = 1; i <= count; i++)
            {
                apprenticeships.Add(new Apprenticeship
                {
                    Id = i,
                    PaymentStatus = PaymentStatus.Active
                });
            }

            return apprenticeships;
        }
    }
}