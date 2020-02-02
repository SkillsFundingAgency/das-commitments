using System;
using System.Collections.Generic;
using System.Text;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Infrastructure;
using StructureMap;

namespace SFA.DAS.CommitmentsV2.DependencyResolution
{
    public class CachingRegistry : Registry
    {
        public CachingRegistry()
        {
            For<ICacheStorageService>().Use<CacheStorageService>();
        }
    }
}
