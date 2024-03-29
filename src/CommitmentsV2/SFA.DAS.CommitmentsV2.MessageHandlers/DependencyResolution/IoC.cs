﻿using Microsoft.Extensions.Configuration;
using SFA.DAS.CommitmentsV2.Configuration;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.DependencyResolution;
using SFA.DAS.CommitmentsV2.Shared.DependencyInjection;
using SFA.DAS.PAS.Account.Api.ClientV2.Configuration;
using SFA.DAS.PAS.Account.Api.ClientV2.DependencyResolution;
using SFA.DAS.ReservationsV2.Api.Client.DependencyResolution;
using SFA.DAS.UnitOfWork.EntityFrameworkCore.DependencyResolution.StructureMap;
using SFA.DAS.UnitOfWork.NServiceBus.DependencyResolution.StructureMap;
using StructureMap;
using EncodingRegistry = SFA.DAS.CommitmentsV2.DependencyResolution.EncodingRegistry;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.DependencyResolution
{
    public static class IoC
    {
        public static void Initialize(Registry registry)
        {
            registry.IncludeRegistry<ConfigurationRegistry>();
            registry.IncludeRegistry<CurrentDateTimeRegistry>();
            registry.IncludeRegistry<DataRegistry>();
            registry.IncludeRegistry<EntityFrameworkCoreUnitOfWorkRegistry<ProviderCommitmentsDbContext>>();
            registry.IncludeRegistry<MediatorRegistry>();
            registry.IncludeRegistry<NServiceBusUnitOfWorkRegistry>();
            registry.IncludeRegistry(new PasAccountApiClientRegistry(context => GetPasConfiguration(context.GetInstance<IConfiguration>())));
            registry.IncludeRegistry<EncodingRegistry>();
            registry.IncludeRegistry<DiffServiceRegistry>();
            registry.IncludeRegistry<EmployerAccountsRegistry>();
            registry.IncludeRegistry<ReservationsApiClientRegistry>();
            registry.IncludeRegistry<DomainServiceRegistry>();
            registry.IncludeRegistry<DefaultRegistry>();
            registry.IncludeRegistry<ApprovalsOuterApiServiceRegistry>();
        }

        private static PasAccountApiConfiguration GetPasConfiguration(IConfiguration configuration)
        {
            return configuration.GetSection(CommitmentsConfigurationKeys.ProviderAccountApiConfiguration).Get<PasAccountApiConfiguration>();
        }
    }
}