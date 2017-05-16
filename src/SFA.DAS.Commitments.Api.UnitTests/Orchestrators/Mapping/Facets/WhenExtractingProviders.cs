using System.Collections.Generic;
using System.Linq;

using FluentAssertions;
using NUnit.Framework;

using SFA.DAS.Commitments.Api.Types.Apprenticeship;
using SFA.DAS.Commitments.Api.Types.Apprenticeship.Types;
using SFA.DAS.Commitments.Application.Services;

namespace SFA.DAS.Commitments.Api.UnitTests.Orchestrators.Mapping.Facets
{
    [TestFixture]
    public class WhenExtractingProviders
    {
        private FacetMapper _sut;
        private List<Apprenticeship> _data;
        private ApprenticeshipSearchQuery _userQuery;

        [SetUp]
        public void SetUp()
        {
            _data = new List<Apprenticeship>();

            _userQuery = new ApprenticeshipSearchQuery();
            _sut = new FacetMapper();
        }

        [Test]
        public void ShouldHave3UniqueProviders()
        {
            _data.Add(new Apprenticeship {ProviderName = "Abba 365", ProviderId = 006});
            _data.Add(new Apprenticeship { ProviderName = "Valtech LTD", ProviderId = 007});
            _data.Add(new Apprenticeship { ProviderName = "Valtech LTD", ProviderId = 007});
            _data.Add(new Apprenticeship { ProviderName = "Command & Conquer", ProviderId = 008});

            var result = _sut.BuildFacetes(_data, _userQuery, Originator.Employer);

            result.TrainingProviders.Count.Should().Be(3);
            result.TrainingProviders.Count(m => m.Selected).Should().Be(0);

            result.TrainingProviders.Single(m => m.Data.Id == 006).Data.Name.Should().Be("Abba 365");
            result.TrainingProviders.Single(m => m.Data.Id == 007).Data.Name.Should().Be("Valtech LTD");
            result.TrainingProviders.Single(m => m.Data.Id == 008).Data.Name.Should().Be("Command & Conquer");
        }

        [Test]
        public void ShouldHave3UniqueProvidersAndOneSelected()
        {
            _data.Add(new Apprenticeship { ProviderName = "Abba 365", ProviderId = 006 });
            _data.Add(new Apprenticeship { ProviderName = "Valtech LTD", ProviderId = 007 });
            _data.Add(new Apprenticeship { ProviderName = "Valtech LTD", ProviderId = 007 }); 
            _data.Add(new Apprenticeship { ProviderName = "Command & Conquer", ProviderId = 008 });

            _userQuery.TrainingProviderIds = new List<long> { 008, 001 };

            var result = _sut.BuildFacetes(_data, _userQuery, Originator.Employer);

            result.TrainingProviders.Count.Should().Be(3);
            result.TrainingProviders.Count(m => m.Selected).Should().Be(1);

            result.TrainingProviders.Single(m => m.Data.Id == 008).Data.Name.Should().Be("Command & Conquer");
            result.TrainingProviders.Single(m => m.Data.Id == 008).Selected.Should().BeTrue();

            result.TrainingProviders.Single(m => m.Data.Id == 006).Selected.Should().BeFalse();
            result.TrainingProviders.Single(m => m.Data.Id == 007).Selected.Should().BeFalse();
        }

    }
}
