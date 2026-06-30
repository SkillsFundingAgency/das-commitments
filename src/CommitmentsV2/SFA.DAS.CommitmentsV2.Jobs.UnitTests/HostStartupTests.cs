using System;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Shared.Startup;
using SFA.DAS.CommitmentsV2.Startup;

namespace SFA.DAS.CommitmentsV2.Jobs.UnitTests;

public class HostStartupTests
{
    private const string ApplicationInsightsConnectionString =
        "InstrumentationKey=00000000-0000-0000-0000-000000000000;IngestionEndpoint=https://westeurope-1.in.applicationinsights.azure.com/";

    private string _previousApplicationInsightsConnectionString;
    private string _previousEnvironmentName;
    private string _previousAzureWebJobsStorage;

    [SetUp]
    public void SetUp()
    {
        _previousApplicationInsightsConnectionString = Environment.GetEnvironmentVariable("APPLICATIONINSIGHTS_CONNECTION_STRING");
        _previousEnvironmentName = Environment.GetEnvironmentVariable("APPSETTING_EnvironmentName");
        _previousAzureWebJobsStorage = Environment.GetEnvironmentVariable("AzureWebJobsStorage");

        Environment.SetEnvironmentVariable("APPLICATIONINSIGHTS_CONNECTION_STRING", ApplicationInsightsConnectionString);
        Environment.SetEnvironmentVariable("APPSETTING_EnvironmentName", "LOCAL");
        Environment.SetEnvironmentVariable("AzureWebJobsStorage", "UseDevelopmentStorage=true");
    }

    [TearDown]
    public void TearDown()
    {
        Environment.SetEnvironmentVariable("APPLICATIONINSIGHTS_CONNECTION_STRING", _previousApplicationInsightsConnectionString);
        Environment.SetEnvironmentVariable("APPSETTING_EnvironmentName", _previousEnvironmentName);
        Environment.SetEnvironmentVariable("AzureWebJobsStorage", _previousAzureWebJobsStorage);
    }

    [Test]
    public void Build_does_not_fail_when_application_insights_is_configured()
    {
        using var host = new HostBuilder()
            .UseDasEnvironment()
            .ConfigureDasAppConfiguration([])
            .ConfigureDasOpenTelemetry()
            .Build();

        Assert.That(host, Is.Not.Null);
    }
}
