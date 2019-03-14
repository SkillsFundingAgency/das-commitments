using System.Collections.Generic;

namespace SFA.DAS.Commitments.Support.SubSite.DependencyResolution
{
    public interface ILoggingPropertyFactory
    {
        IDictionary<string, object> GetProperties();
    }
}