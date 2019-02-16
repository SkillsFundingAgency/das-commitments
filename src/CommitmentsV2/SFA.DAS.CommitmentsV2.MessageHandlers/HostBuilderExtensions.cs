//using System;
//using System.Collections.Generic;
//using System.Text;
//using Microsoft.Azure.WebJobs.Host.Config;
//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Hosting;
//using StructureMap;

//namespace SFA.DAS.CommitmentsV2.MessageHandlers
//{
//    public static class HostBuilderExtensions
//    {
//        public static IHostBuilder ConfigureDasAppConfiguration(this IHostBuilder builder, string[] args)
//        {
//            return builder.ConfigureAppConfiguration((c, b) => b
//                //.AddAzureTableStorage(EmployerFinanceConfigurationKeys.Base)
//                .AddJsonFile("appsettings.json", true, true)
//                .AddJsonFile($"appsettings.{c.HostingEnvironment.EnvironmentName}.json", true, true)
//                .AddEnvironmentVariables());
//                //.AddCommandLine(args));
//        }

//        //public static IHostBuilder ConfigureDasLogging(this IHostBuilder builder)
//        //{
//        //    return builder.ConfigureLogging(b => b.AddNLog());
//        //}

//        public static IHostBuilder ConfigureDasWebJobs(this IHostBuilder builder)
//        {
//            builder.ConfigureWebJobs(b => b.AddAzureStorageCoreServices().AddTimers());

//#pragma warning disable 618
//            builder.ConfigureServices(s => s.AddSingleton<IWebHookProvider>(p => null));
//#pragma warning restore 618

//            return builder;
//        }

//        public static IHostBuilder UseDasEnvironment(this IHostBuilder hostBuilder)
//        {
//            var environmentName = Environment.GetEnvironmentVariable(EnvironmentVariableNames.EnvironmentName);
//            var mappedEnvironmentName = DasEnvironmentName.Map[environmentName];

//            return hostBuilder.UseEnvironment(mappedEnvironmentName);
//        }

//        public static IHostBuilder UseStructureMap(this IHostBuilder builder)
//        {
//            return builder.UseServiceProviderFactory(new StructureMapServiceProviderFactory(null));
//        }
//    }
