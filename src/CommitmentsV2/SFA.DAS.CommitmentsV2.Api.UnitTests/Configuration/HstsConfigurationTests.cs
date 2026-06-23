using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace SFA.DAS.CommitmentsV2.Api.UnitTests.Configuration;

[TestFixture]
public class HstsConfigurationTests
{
    [Test]
    public void AddHsts_registers_MaxAge_of_365_days()
    {
        var services = new ServiceCollection();
        services.AddHsts(options => options.MaxAge = TimeSpan.FromDays(365));

        var options = services.BuildServiceProvider()
            .GetRequiredService<IOptions<HstsOptions>>()
            .Value;

        options.MaxAge.Should().Be(TimeSpan.FromDays(365));
        options.MaxAge.TotalSeconds.Should().Be(31_536_000);
    }

    [Test]
    public async Task UseHsts_adds_Strict_Transport_Security_header_with_365_day_max_age()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddHsts(options => options.MaxAge = TimeSpan.FromDays(365));
        var serviceProvider = services.BuildServiceProvider();

        var appBuilder = new ApplicationBuilder(serviceProvider);
        appBuilder.UseHsts();

        var context = new DefaultHttpContext
        {
            RequestServices = serviceProvider
        };
        context.Request.Scheme = "https";
        context.Request.Host = new HostString("commitments-api.example.com");

        RequestDelegate pipeline = appBuilder.Build();
        await pipeline(context);

        context.Response.Headers["Strict-Transport-Security"].ToString()
            .Should()
            .Contain("max-age=31536000");
    }
}
