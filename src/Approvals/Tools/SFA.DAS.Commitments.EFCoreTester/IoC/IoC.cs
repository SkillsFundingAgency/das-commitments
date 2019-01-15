using StructureMap;

namespace SFA.DAS.Commitments.EFCoreTester.IoC
{
    public static class IoC
    {
        public static IContainer InitialiseIoC(string configLocation)
        {
            return new Container(c =>
            {
                c.AddRegistry(new CommonRegistry(configLocation));
            });
        }
    }
}
