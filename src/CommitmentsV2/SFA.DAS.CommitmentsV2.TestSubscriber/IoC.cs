using StructureMap;

namespace SFA.DAS.CommitmentsV2.TestSubscriber
{
    public static class IoC
    {
        public static IContainer InitializeIoC()
        {
            return new Container(c =>
            {
                c.AddRegistry<DefaultRegistry>();
            });
        }
    }
}