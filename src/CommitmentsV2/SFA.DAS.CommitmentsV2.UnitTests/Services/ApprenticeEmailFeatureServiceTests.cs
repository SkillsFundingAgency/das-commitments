using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Authorization.Features.Models;
using SFA.DAS.Authorization.Features.Services;
using SFA.DAS.CommitmentsV2.Configuration;
using SFA.DAS.CommitmentsV2.Services;

namespace SFA.DAS.CommitmentsV2.UnitTests.Services
{
    [TestFixture]
    public class ApprenticeEmailFeatureServiceTests
    {
        private Fixture autoFixture = new Fixture();

        [TestCase(false)]
        [TestCase(true)]
        public void WhenNoApprenticeEmailFeatureExists_ThenIsEnabledReturnsFalseRegardlessOfPrivateBetaValues(bool usePrivateBetaList)
        {
            var feature = new CheckingApprenticeEmailFeatureFixture(usePrivateBetaList, autoFixture.CreateMany<PrivateBetaItem>().ToList(), null);

            feature.ApprenticeEmailFeatureService.IsEnabled.Should().BeFalse();
        }

        [TestCase(false)]
        [TestCase(true)]
        public void WhenApprenticeEmailFeatureExists_ThenIsEnabledReturnsExpectedResult(bool featureState)
        {
            var feature = new CheckingApprenticeEmailFeatureFixture(autoFixture.Create<bool>(), autoFixture.CreateMany<PrivateBetaItem>().ToList(), CreateApprenticeEmailFeatureToggle(featureState));

            feature.ApprenticeEmailFeatureService.IsEnabled.Should().Be(featureState);
        }

        [Test]
        public void WhenApprenticeEmailFeatureIsOn_AndUsePrivateBetaListIsFalseThenAllEmployerAndProvidersRequireEmail()
        {
            var feature = new CheckingApprenticeEmailFeatureFixture(false, autoFixture.CreateMany<PrivateBetaItem>().ToList(), CreateApprenticeEmailFeatureToggle(true));

            feature.ApprenticeEmailFeatureService.ApprenticeEmailIsRequiredFor(autoFixture.Create<long>(), autoFixture.Create<long>()).Should().BeTrue();
        }

        [TestCase(1,1,true)]
        [TestCase(1,2,true)]
        [TestCase(1,3,false)]
        [TestCase(3,1,true)]
        [TestCase(3,4,true)]
        [TestCase(2,4,false)]
        public void WhenApprenticeEmailFeatureIsOn_AndUsePrivateBetaListIsTrueThenOnlyEmployerAndProvidersInListRequireEmail(long employerAccountId, long providerId, bool expected)
        {
            var privateBetaList = new List<PrivateBetaItem>
            {
                new PrivateBetaItem { EmployerAccountId = 1, ProviderId = 1 },
                new PrivateBetaItem { EmployerAccountId = 1, ProviderId = 2 },
                new PrivateBetaItem { EmployerAccountId = 3, ProviderId = 1 },
                new PrivateBetaItem { EmployerAccountId = 3, ProviderId = 4 }
            };

            var feature = new CheckingApprenticeEmailFeatureFixture(true, privateBetaList, CreateApprenticeEmailFeatureToggle(true));

            feature.ApprenticeEmailFeatureService.ApprenticeEmailIsRequiredFor(employerAccountId, providerId).Should().Be(expected);
        }

        private FeatureToggle CreateApprenticeEmailFeatureToggle(bool state)
        {
            return new FeatureToggle { Feature = "ApprenticeEmail", IsEnabled = state };
        }

        private class CheckingApprenticeEmailFeatureFixture
        {
            public ApprenticeEmailFeatureService ApprenticeEmailFeatureService { get; }
            public CustomisedFeaturesConfiguration CustomisedFeaturesConfiguration { get; }
            public FeatureTogglesService<CustomisedFeaturesConfiguration, FeatureToggle> FeatureToogle { get; set; }

            public CheckingApprenticeEmailFeatureFixture(bool usePrivateBetaList, List<PrivateBetaItem> list, FeatureToggle feature)
            {
                CustomisedFeaturesConfiguration = new CustomisedFeaturesConfiguration();
                CustomisedFeaturesConfiguration.ApprenticeEmailFeatureUsePrivateBetaList = usePrivateBetaList;
                CustomisedFeaturesConfiguration.PrivateBetaList = list;
                CustomisedFeaturesConfiguration.FeatureToggles = new List<FeatureToggle>();
                if (feature != null)
                {
                    CustomisedFeaturesConfiguration.FeatureToggles.Add(feature);
                }

                FeatureToogle = new FeatureTogglesService<CustomisedFeaturesConfiguration, FeatureToggle>(CustomisedFeaturesConfiguration);
                
                ApprenticeEmailFeatureService = new ApprenticeEmailFeatureService(FeatureToogle, CustomisedFeaturesConfiguration);
            }
        }
    }
}
