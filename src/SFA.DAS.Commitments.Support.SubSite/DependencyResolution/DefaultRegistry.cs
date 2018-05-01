// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DefaultRegistry.cs" company="Web Advanced">
// Copyright 2012 Web Advanced (www.webadvanced.com)
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0

// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace SFA.DAS.Commitments.Support.SubSite.DependencyResolution
{
    using MediatR;
    using Microsoft.Azure;
    using SFA.DAS.Commitments.Domain.Data;
    using SFA.DAS.Commitments.Domain.Interfaces;
    using SFA.DAS.Commitments.Infrastructure.Data;
    using SFA.DAS.Commitments.Infrastructure.Logging;
    using SFA.DAS.Commitments.Support.SubSite.Configuration;
    using SFA.DAS.Commitments.Support.SubSite.Orchestrators;
    using SFA.DAS.Configuration;
    using SFA.DAS.Configuration.AzureTableStorage;
    using SFA.DAS.Learners.Validators;
    using SFA.DAS.NLog.Logger;
    using StructureMap;
    using StructureMap.Configuration.DSL;
    using StructureMap.Graph;
    using System.Web;

    public class DefaultRegistry : Registry
    {
        private const string ServiceName = "SFA.DAS.Support.Commitments";
        private const string Version = "1.0";

        public DefaultRegistry()
        {
            Scan(
                scan =>
                {
                    scan.TheCallingAssembly();
                    scan.WithDefaultConventions();
                });

            ConfigureLog();
        }

        private void ConfigureLog()
        {

            var config = GetConfiguration();

            For<IUlnValidator>().Use<UlnValidator>();
            For<ICommitmentRepository>().Use<CommitmentRepository>().Ctor<string>().Is(config.DatabaseConnectionString);
            For<IApprenticeshipRepository>().Use<ApprenticeshipRepository>().Ctor<string>().Is(config.DatabaseConnectionString);

            For<ILoggingPropertyFactory>().Use<LoggingPropertyFactory>();
            HttpContextBase conTextBase = null;
            if (HttpContext.Current != null)
            {
                conTextBase = new HttpContextWrapper(HttpContext.Current);
            }

            For<IWebLoggingContext>().Use(x => new WebLoggingContext(conTextBase));
            For<ILog>().Use(x => new NLogLogger(
                x.ParentType,
                x.GetInstance<IWebLoggingContext>(),
                x.GetInstance<ILoggingPropertyFactory>().GetProperties())).AlwaysUnique();

            For<ICommitmentsLogger>().Use(x => new CommitmentsLogger(x.GetInstance<ILog>())).AlwaysUnique();

            For<SingleInstanceFactory>().Use<SingleInstanceFactory>(ctx => t => ctx.GetInstance(t));
            For<MultiInstanceFactory>().Use<MultiInstanceFactory>(ctx => t => ctx.GetAllInstances(t));
            For<IMediator>().Use<Mediator>();

            For<IApprenticeshipsOrchestrator>().Use<ApprenticeshipsOrchestrator>();

        }

        private CommitmentSupportSiteConfiguartion GetConfiguration()
        {
            var environment = CloudConfigurationManager.GetSetting("EnvironmentName") ?? "LOCAL";
            var storageConnectionString = CloudConfigurationManager.GetSetting("ConfigurationStorageConnectionString") ?? "UseDevelopmentStorage=true;";
            var configurationRepository = new AzureTableStorageConfigurationRepository(storageConnectionString);
            var configurationOptions = new ConfigurationOptions(ServiceName, environment, Version);
            var configurationService = new ConfigurationService(configurationRepository, configurationOptions);
            var webConfiguration = configurationService.Get<CommitmentSupportSiteConfiguartion>();

            return webConfiguration;
        }

    }
}