﻿using Moq;
using NUnit.Framework;
using SFA.DAS.AutoConfiguration;
using SFA.DAS.CommitmentsV2.Configuration;
using SFA.DAS.CommitmentsV2.LinkGeneration;

namespace SFA.DAS.CommitmentsV2.UnitTests.LinkGeneration
{
    [TestFixture]
    public class LinkGeneratorTests
    {
        [TestCase("base", "path", "base/path")]
        [TestCase("base/", "path", "base/path")]
        [TestCase("base", "/path", "base/path")]
        [TestCase("base/", "/path", "base/path")]
        public void ProviderCommitmentsLink_(string providerApprenticeshipServiceUrl, string path, string expectedUrl)
        {
            var fixtures = new LinkGeneratorTestFixtures()
                .WithProviderApprenticeshipServiceBaseUrl(providerApprenticeshipServiceUrl);

            var actualUrl = fixtures.GetProviderApprenticeshipServiceLink(path);

            Assert.AreEqual(expectedUrl, actualUrl);
        }

        [TestCase("base", "path", "base/path")]
        [TestCase("base/", "path", "base/path")]
        [TestCase("base", "/path", "base/path")]
        [TestCase("base/", "/path", "base/path")]
        public void CourseManagementLink_(string providerApprenticeshipServiceUrl, string path, string expectedUrl)
        {
            var fixtures = new LinkGeneratorTestFixtures()
                .WithProviderApprenticeshipServiceBaseUrl(providerApprenticeshipServiceUrl);

            var actualUrl = fixtures.GetCourseManagementLink(path);

            Assert.AreEqual(expectedUrl, actualUrl);
        }
    }

    public class LinkGeneratorTestFixtures
    {
        public LinkGeneratorTestFixtures()
        {
            _providerUrlConfiguration = new ProviderUrlConfiguration();
        }

        public ProviderUrlConfiguration _providerUrlConfiguration { get; }
       

        public LinkGeneratorTestFixtures WithProviderApprenticeshipServiceBaseUrl(string baseUrl)
        {
            _providerUrlConfiguration.ProviderApprenticeshipServiceBaseUrl = baseUrl;
            _providerUrlConfiguration.CourseManagementBaseUrl = baseUrl;
            return this;
        }

        public string GetProviderApprenticeshipServiceLink(string path)
        {
            var linkGenerator = new LinkGenerator(_providerUrlConfiguration);
            return linkGenerator.ProviderApprenticeshipServiceLink(path);
        }

        public string GetCourseManagementLink(string path)
        {
            var linkGenerator = new LinkGenerator(_providerUrlConfiguration);
            return linkGenerator.CourseManagementLink(path);
        }
    }
}