using System.Collections.Generic;
using System.Linq;

using FluentAssertions;
using NUnit.Framework;

using SFA.DAS.Commitments.Api.Models;
using SFA.DAS.Commitments.Api.Orchestrators.Mappers;
using SFA.DAS.Commitments.Api.Types.Apprenticeship;
using SFA.DAS.Commitments.Api.Types.Apprenticeship.Types;

namespace SFA.DAS.Commitments.Api.UnitTests.Orchestrators.Mapping.Facets
{
    [TestFixture]
    public class WhenExtractingProviders
    {
        private FacetMapper _sut;
        private List<Apprenticeship> _data;
        private ApprenticeshipQuery _userQuery;

        [SetUp]
        public void SetUp()
        {
            _data = new List<Apprenticeship>();

            _userQuery = new ApprenticeshipQuery();
            _sut = new FacetMapper();
        }

        [Test]
        public void ShouldHave3UniqueProviders()
        {
            _data.Add(new Apprenticeship {ProviderName = "Abba 365"});
            _data.Add(new Apprenticeship { ProviderName = "Command & Conquer" });
            _data.Add(new Apprenticeship { ProviderName = "Valtech LTD" });
            _data.Add(new Apprenticeship { ProviderName = "Valtech LTD" });

            var result = _sut.BuildFacetes(_data, _userQuery, Originator.Employer);

            result.TrainingProviders.Count.Should().Be(3);
            result.TrainingProviders.Count(m => m.Selected).Should().Be(0);

            result.TrainingProviders[0]?.Data.Should().Be("Abba 365");
            result.TrainingProviders[1]?.Data.Should().Be("Command & Conquer");
            result.TrainingProviders[2]?.Data.Should().Be("Valtech LTD");
        }

        [Test]
        public void ShouldHave3UniqueProvidersAndOneSelected()
        {
            _data.Add(new Apprenticeship { ProviderName = "Abba 365" });
            _data.Add(new Apprenticeship { ProviderName = "Command & Conquer" });
            _data.Add(new Apprenticeship { ProviderName = "Valtech LTD" });
            _data.Add(new Apprenticeship { ProviderName = "Valtech LTD" }); 

            _userQuery.TrainingProviders = new List<string> { "Tottenham FC", "Command & Conquer" };

            var result = _sut.BuildFacetes(_data, _userQuery, Originator.Employer);

            result.TrainingProviders.Count.Should().Be(3);
            result.TrainingProviders.Count(m => m.Selected).Should().Be(1);

            result.TrainingProviders[1]?.Data.Should().Be("Command & Conquer");
            result.TrainingProviders[1]?.Selected.Should().BeTrue();

            result.TrainingProviders[0]?.Selected.Should().BeFalse();
            result.TrainingProviders[2]?.Selected.Should().BeFalse();

        }

    }
}
