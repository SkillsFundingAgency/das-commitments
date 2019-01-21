using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using StructureMap;

namespace SFA.DAS.ProviderCommitments.Web.DependencyResolution
{
    public class DefaultRegistry : Registry
    {

        public DefaultRegistry()
        {

            Scan(s =>
            {
                s.AssembliesFromApplicationBaseDirectory(a => a.GetName().Name.StartsWith("SFA.DAS"));
                s.With(new ControllerConvention());
            });
        }

    }
}