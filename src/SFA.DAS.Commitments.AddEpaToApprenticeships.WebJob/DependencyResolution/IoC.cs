using StructureMap;
using System.Diagnostics;

namespace SFA.DAS.Commitments.AddEpaToApprenticeships.WebJob.DependencyResolution
{
    public static class IoC
    {
        public static IContainer Initialize()
        {
            var container = new Container(c =>
            {
                c.AddRegistry<DefaultRegistry>();
                c.AddRegistry<PaymentsRegistry>();
            });

            //Debug.WriteLine(container.WhatDidIScan());
            //container.AssertConfigurationIsValid();

            return container;
        }
    }
}
