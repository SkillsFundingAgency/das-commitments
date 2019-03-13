using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using SFA.DAS.CommitmentsV2.Configuration;
using SFA.DAS.Configuration.AzureTableStorage;

namespace SFA.DAS.CommitmentsV2.Api.Extensions
{
    public static class IWebHostBuilderExtensions
    {
        public static IWebHostBuilder ConfigureDasAppConfiguration(this IWebHostBuilder hostBuilder)
        {
            return hostBuilder.ConfigureAppConfiguration(c => c
                .AddAzureTableStorage(CommitmentsConfigurationKeys.CommitmentsV2Base));
        }
    }
}
