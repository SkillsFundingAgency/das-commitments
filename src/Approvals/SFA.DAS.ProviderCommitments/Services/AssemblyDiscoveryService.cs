using System;
using System.Linq;
using SFA.DAS.ProviderCommitments.Interfaces;

namespace SFA.DAS.ProviderCommitments.Services
{
    public class AssemblyDiscoveryService : IAssemblyDiscoveryService
    {
        public Type[] GetApplicationTypes(string matchingClassName)
        {
            return AppDomain.CurrentDomain
                .GetAssemblies()
                .Where(assembly => assembly.GetName().Name.StartsWith(Constants.AssemblyPrefixForApplication, StringComparison.InvariantCultureIgnoreCase))
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => string.Equals(type.Name, matchingClassName, StringComparison.InvariantCultureIgnoreCase)).ToArray();
        }
    }
}