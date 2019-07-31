using System;
using Microsoft.Azure;
using Newtonsoft.Json.Linq;
using SFA.DAS.Commitments.Domain.Interfaces;
using SFA.DAS.Configuration.AzureTableStorage;
using SFA.DAS.Reservations.Api.Types.Configuration;
using StructureMap;

namespace SFA.DAS.Commitments.Api.DependencyResolution
{
    public class ReservationsApiClientRegistry : Registry
    {
        private const string ServiceName = "SFA.DAS.CommitmentsV2";
        private const string Version = "1.0";

        public ReservationsApiClientRegistry()
        {
            For<ReservationsClientApiConfiguration>().Use(ctx => RegisterConfig(ctx));
            IncludeRegistry<SFA.DAS.Reservation.Api.Client.DependencyResolution.ReservationsApiClientRegistry>();
        }

        private ReservationsClientApiConfiguration RegisterConfig(IContext context)
        {
            const string reservationClientApi = "ReservationsClientApi";
            var logger = context.GetInstance<ICommitmentsLogger>();
            try
            {
                // The reservations config is stored in V2 of commitments. Rather than duplicate in V2 we're
                // going to read it in V1. 
                var version2ConfigString = GetVersion2ConfigString(logger);

                var reservationsConfig =
                    GetConfigItemFromNamedProperty<ReservationsClientApiConfiguration>(
                        version2ConfigString,
                        reservationClientApi);

                logger.Debug($"Using end point {reservationsConfig.ApiBaseUrl} for reservations");
                return reservationsConfig;
            }
            catch (Exception ex)
            {
                logger.Fatal(ex, $"Failed to obtain V2 config or convert the value for property {reservationClientApi} to type {nameof(ReservationsClientApiConfiguration)}");
                throw;
            }
        }

        private string GetVersion2ConfigString(ICommitmentsLogger logger)
        {
            var environment = CloudConfigurationManager.GetSetting("EnvironmentName");

            var configurationRepository = new AzureTableStorageConfigurationRepository(CloudConfigurationManager.GetSetting("ConfigurationStorageConnectionString"));
            var configString = configurationRepository.Get(ServiceName, environment, Version);

            logger.Debug($"Retrieved config for V2 - length {configString.Length}");
            return configString;
        }

        private T GetConfigItemFromNamedProperty<T>(string configString, string propertyName) 
        {
            var jObject = JObject.Parse(configString);
            var configProperty = jObject.GetValue(propertyName);

            if (configProperty == null)
            {
                throw new InvalidOperationException($"The property {propertyName} does not exist in the Commitments V2 config - cannot initialise the reservations API client");
            }

            var configItem = configProperty.ToObject<T>();
            return configItem;
        }
    }
}