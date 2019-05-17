using SFA.DAS.Encoding;
using StructureMap;

namespace SFA.DAS.CommitmentsV2.DependencyResolution
{
    public class EncodingRegistry : Registry
    {
        public EncodingRegistry()
        {
            For<IEncodingService>().Use<EncodingService>().Singleton();
        }
    }
}
