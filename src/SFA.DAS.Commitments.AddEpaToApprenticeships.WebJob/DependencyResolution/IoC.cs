using StructureMap;
//using StructureMap.Graph.Scanning;
using System.Diagnostics;

namespace SFA.DAS.Commitments.AddEpaToApprenticeships.WebJob.DependencyResolution
{
    public static class IoC
    {
        public static IContainer Initialize()
        {
            //TypeRepository.AssertNoTypeScanningFailures();

            var container = new Container(c =>
            {
                c.AddRegistry<DefaultRegistry>();
            });

            //Debug.WriteLine(container.WhatDidIScan());

            return container;
        }
    }
}
