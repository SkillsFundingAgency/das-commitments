using System;
using System.Linq;
using SFA.DAS.ProviderCommitments.Interfaces;

namespace SFA.DAS.ProviderCommitments.Services
{
    public class AssemblyDiscoveryService : IAssemblyDiscoveryService
    {
        public Type[] GetApplicationTypes(string matchingClassName)
        {
            return GetApplicationTypes(Constants.AssemblyPrefixForApplication, matchingClassName);
        }

        public Type[] GetApplicationTypes(string matchingAssemblyName, string matchingClassName)
        {
            var lastWord = matchingClassName.LastIndexOf('.');

            string nameSpace, className;

            if (lastWord > -1)
            {
                nameSpace = matchingClassName.Substring(0, lastWord);
                className = matchingClassName.Substring(lastWord + 1);
            }
            else
            {
                nameSpace = null;
                className = matchingClassName;
            }

            return AppDomain.CurrentDomain
                .GetAssemblies()
                .Where(assembly => assembly.GetName().Name.StartsWith(matchingAssemblyName, StringComparison.InvariantCultureIgnoreCase))
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => className.Equals(type.Name, StringComparison.InvariantCultureIgnoreCase) 
                               && (nameSpace == null || (type.Namespace != null && type.Namespace.EndsWith(nameSpace, StringComparison.InvariantCultureIgnoreCase))))
                .ToArray();
        }
    }
}