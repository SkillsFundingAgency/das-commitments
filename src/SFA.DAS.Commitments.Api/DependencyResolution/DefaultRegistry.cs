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

using System.Web;
using FluentValidation;
using MediatR;

using SFA.DAS.Commitments.Application.Services;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities.TrainingProgramme;
using SFA.DAS.Commitments.Domain.Interfaces;
using SFA.DAS.Commitments.Infrastructure.Configuration;
using SFA.DAS.Commitments.Infrastructure.Data;
using SFA.DAS.Commitments.Infrastructure.Logging;
using SFA.DAS.EAS.Account.Api.Client;
using SFA.DAS.Events.Api.Client;
using SFA.DAS.Events.Api.Client.Configuration;
using SFA.DAS.NLog.Logger;
using StructureMap;
using SFA.DAS.Learners.Validators;
using SFA.DAS.Commitments.Infrastructure.Services;
using SFA.DAS.HashingService;
using Microsoft.Extensions.Logging;

namespace SFA.DAS.Commitments.Api.DependencyResolution
{
    public class DefaultRegistry : Registry
    {
        private const string ServiceName = "SFA.DAS.Commitments";

        public DefaultRegistry()
        {
            Scan(
                scan =>
                {
                    scan.AssembliesFromApplicationBaseDirectory(a => a.GetName().Name.StartsWith(ServiceName));
                    scan.RegisterConcreteTypesAgainstTheFirstInterface();
                    scan.ConnectImplementationsToTypesClosing(typeof(AbstractValidator<>));
                    scan.ConnectImplementationsToTypesClosing(typeof(IRequestHandler<,>));
                    scan.ConnectImplementationsToTypesClosing(typeof(IAsyncRequestHandler<,>));
                    scan.ConnectImplementationsToTypesClosing(typeof(INotificationHandler<>));
                    scan.ConnectImplementationsToTypesClosing(typeof(IAsyncNotificationHandler<>));
                });

            var config = Infrastructure.Configuration.Configuration.Get();


            ConfigureHashingService(config);

            For<IAccountApiClient>().Use<AccountApiClient>().Ctor<IAccountApiConfiguration>().Is(config.AccountApi);

            For<IEventsApi>().Use<DummyEventsApiClient>();

            For<IAcademicYearDateProvider>().Use<AcademicYearDateProvider>();
            For<IUlnValidator>().Use<UlnValidator>();
            For<IAcademicYearValidator>().Use<AcademicYearValidator>();

            For<ICommitmentRepository>().Use<CommitmentRepository>().Ctor<string>().Is(config.DatabaseConnectionString);
            For<IApprenticeshipRepository>().Use<ApprenticeshipRepository>().Ctor<string>().Is(config.DatabaseConnectionString);
            For<IApprenticeshipUpdateRepository>().Use<ApprenticeshipUpdateRepository>().Ctor<string>().Is(config.DatabaseConnectionString);
            For<IDataLockRepository>().Use<DataLockRepository>().Ctor<string>().Is(config.DatabaseConnectionString);
            For<IHistoryRepository>().Use<HistoryRepository>().Ctor<string>().Is(config.DatabaseConnectionString);
            For<IBulkUploadRepository>().Use<BulkUploadRepository>().Ctor<string>().Is(config.DatabaseConnectionString);
            For<IStatisticsRepository>().Use<StatisticsRepository>().Ctor<string>().Is(config.DatabaseConnectionString);
            For<ITrainingProgrammeRepository>().Use<TrainingProgrammeRepository>().Ctor<string>().Is(config.DatabaseConnectionString);
            For<IProviderRepository>().Use<ProviderRepository>().Ctor<string>().Is(config.DatabaseConnectionString);

            For<SingleInstanceFactory>().Use<SingleInstanceFactory>(ctx => t => ctx.GetInstance(t));
            For<MultiInstanceFactory>().Use<MultiInstanceFactory>(ctx => t => ctx.GetAllInstances(t));
            For<IMediator>().Use<Mediator>();

            For<ICache>().Use<InMemoryCache>();

            ConfigureLogging();
        }

        private void ConfigureLogging()
        {
            For<ILoggingContext>().Use(x => new WebLoggingContext(new HttpContextWrapper(HttpContext.Current)));
            For<ILog>().Use(c => new NLogLogger(c.ParentType, c.GetInstance<ILoggingContext>(), null)).AlwaysUnique();
            For<ICommitmentsLogger>().Use(x => GetBaseLogger(x)).AlwaysUnique();

            For<ILoggerFactory>().Use(ctx => CreateLoggerFactory()).Singleton();
        }

        private ILoggerFactory CreateLoggerFactory()
        {
            var loggerFactory = new LoggerFactory();

            return loggerFactory;
        }

        private ICommitmentsLogger GetBaseLogger(IContext x)
        {
            var parentType = x.ParentType;
            return new CommitmentsLogger(new NLogLogger(parentType, x.GetInstance<ILoggingContext>()));
        }

        private void ConfigureHashingService(CommitmentsApiConfiguration config)
        {
            For<IHashingService>().Use(x => new HashingService.HashingService(config.AllowedHashstringCharacters, config.Hashstring));
        }
    }
}
