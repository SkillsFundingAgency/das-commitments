using System;
using System.Configuration;
using System.Linq;
using Microsoft.Azure;
using SFA.DAS.Commitments.Domain.Interfaces;
using SFA.DAS.Commitments.Infrastructure.Services;
using StructureMap;
using StructureMap.Pipeline;

namespace SFA.DAS.Commitments.Infrastructure.Configuration
{
    public class CurrentDatePolicy : ConfiguredInstancePolicy
    {
        protected override void apply(Type pluginType, IConfiguredInstance instance)
        {
            var constructorParameter =
                instance?.Constructor?.GetParameters().FirstOrDefault(x => x.ParameterType == typeof(ICurrentDateTime));

            if (constructorParameter != null)
            {
                var param = new CurrentDateTime(GetCurrentTimeFromConfiguration());
                instance.Dependencies.AddForConstructorParameter(constructorParameter, param);
            }
        }

        private DateTime? GetCurrentTimeFromConfiguration()
        {
            var cloudCurrentTime = ConfigurationManager.AppSettings["CurrentTime"];

            var result = default(DateTime?);
            DateTime parsedTime;

            if (DateTime.TryParse(cloudCurrentTime, out parsedTime))
            {
                result = parsedTime;
            }

            return result;
        }
    }
}
