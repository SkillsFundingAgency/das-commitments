using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SFA.DAS.NLog.Logger;
using StructureMap;

namespace SFA.DAS.Commitments.AddEpaToApprenticeships.WebJob.DependencyResolution
{
    public class DefaultRegistry : Registry
    {
        public DefaultRegistry()
        {
            Scan(
                scan =>
                {
                    scan.AssembliesFromApplicationBaseDirectory(a => a.GetName().Name.StartsWith("SFA.DAS"));
                    scan.RegisterConcreteTypesAgainstTheFirstInterface();
                });

            //var config = GetConfiguration("SFA.DAS.CommitmentPayments");

            For<ILog>().Use(x => new NLogLogger(x.ParentType, new ConsoleLoggingContext(), null)).AlwaysUnique();
        }
    }
}
