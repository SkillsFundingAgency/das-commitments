using StructureMap;

namespace SFA.DAS.ProviderCommitments.Web.DependencyResolution
{
    public static class IoC
    {
        public static IContainer Initialize()
        {
            return new Container(c =>
            {
                c.AddRegistry<DefaultRegistry>();
            });
        }

    }
}