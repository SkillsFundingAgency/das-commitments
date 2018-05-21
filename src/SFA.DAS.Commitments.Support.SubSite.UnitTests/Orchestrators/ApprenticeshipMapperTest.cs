using NUnit.Framework;
using SFA.DAS.Commitments.Support.SubSite.Orchestrators;
using FluentValidation;
using FluentValidation.Results;
using FluentAssertions;
using MediatR;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SFA.DAS.Commitments.Support.SubSite.Models;
using SFA.DAS.Commitments.Application.Queries.GetApprenticeshipsByUln;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.HashingService;

namespace SFA.DAS.Commitments.Support.SubSite.UnitTests.Orchestrators
{
    [TestFixture]
    public class ApprenticeshipMapperTest
    {
        private Mock<IHashingService> _hashingService;

        [SetUp]
        public void SetUp()
        {
            _hashingService = new Mock<IHashingService>();


        }

        [Test]
        public void ShouldMapToValidUlnSummaryViewModel()
        {
            var sut = new ApprenticeshipMapper(_hashingService.Object);
            var response = new GetApprenticeshipsByUlnResponse
            {
                TotalCount = 1,
                Apprenticeships = new List<Apprenticeship>
                {
                    new Apprenticeship
                    {
                        FirstName = "Test",
                        LastName = "Me"
                    }
                }
            };

            var result = sut.MapToUlnResultView(response);

            result.Should().NotBeNull();
            result.Should().BeOfType<UlnSearchResultSummaryViewModel>();
            result.ApprenticeshipsCount.Should().Be(1);
        }

        [Test]
        public void ShouldMapToValidApprenticeshipViewModel()
        {
            var sut = new ApprenticeshipMapper(_hashingService.Object);
            var response = new Apprenticeship
            {
                FirstName = "David",
                LastName = "John"
            };

            var result = sut.MapToApprenticeshipViewModel(response);
            result.Should().NotBeNull();
            result.Should().BeOfType<ApprenticeshipViewModel>();
        }



    }
}
