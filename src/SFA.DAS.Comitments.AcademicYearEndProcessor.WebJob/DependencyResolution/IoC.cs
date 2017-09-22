using StructureMap;

namespace SFA.DAS.Comitments.AcademicYearEndProcessor.WebJob.DependencyResolution
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
