using System;
using SFA.DAS.CommitmentsV2.Configuration;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Services;
using StructureMap;

namespace SFA.DAS.CommitmentsV2.DependencyResolution
{
    public class CurrentDateTimeRegistry : Registry
    {
        public CurrentDateTimeRegistry()
        {
            For<ICurrentDateTime>().Use<CurrentDateTime>()
                .Ctor<DateTime?>("value").Is(x => x.GetInstance<CommitmentsV2Configuration>().CurrentDateTime);
        }
    }
}
