using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace SFA.DAS.Commitments.Support.SubSite.DependencyResolution
{
    [ExcludeFromCodeCoverage]
    public class LoggingPropertyFactory : ILoggingPropertyFactory
    {
        public IDictionary<string, object> GetProperties()
        {
            var properties = new Dictionary<string, object>();
            try
            {
                properties.Add("Version", GetVersion());
            }
            catch (Exception)
            {
                throw;
            }

            return properties;
        }

        private string GetVersion()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
            return fileVersionInfo.ProductVersion;
        }
    }
}