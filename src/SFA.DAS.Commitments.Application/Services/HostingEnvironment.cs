using System;
using System.Configuration;
using SFA.DAS.Commitments.Application.Interfaces;

namespace SFA.DAS.Commitments.Application.Services
{
    public class HostingEnvironment : IHostingEnvironment
    {
        private readonly Lazy<(Environment Environment, EnvironmentType EnvironmentType)> _lazyEnvironment; 

        public HostingEnvironment()
        {
            _lazyEnvironment = new Lazy<(Environment, EnvironmentType)>(Initialise);
        }

        public Environment Environment => _lazyEnvironment.Value.Environment;
        public EnvironmentType EnvironmentType => _lazyEnvironment.Value.EnvironmentType;
        public bool IsDevelopment => EnvironmentType == EnvironmentType.Development;
        public bool IsTest => EnvironmentType == EnvironmentType.Test;
        public bool IsProduction => EnvironmentType == EnvironmentType.Production;

        private (Environment, EnvironmentType) Initialise()
        {
            var environmentName = System.Environment.GetEnvironmentVariable("DASENV");

            if (string.IsNullOrEmpty(environmentName))
            {
                environmentName = ConfigurationManager.AppSettings["EnvironmentName"];
            }

            if (!Enum.TryParse(environmentName, true, out Environment environment))
            {
                environment = Environment.Unknown;
            }

            return (environment, MapEnvironmentType(environment));
        }

        private EnvironmentType MapEnvironmentType(Environment environment)
        {
            switch (environment)
            {
                case Environment.Local:
                case Environment.AT: return EnvironmentType.Development;

                case Environment.Test:
                case Environment.Test2: return EnvironmentType.Test;

                case Environment.PreProd:
                case Environment.Prod:
                case Environment.MO: return EnvironmentType.Production;

                default: return EnvironmentType.Unknown;
            }
        }
    }
}