using StructureMap;

namespace SFA.DAS.CommitmentPayments.WebJob.DependencyResolution
{
    public static class IoC
    {
        public static IContainer Initialize()
        {
            return new Container(c =>
            {
                c.AddRegistry<DefaultRegistry>();
                c.AddRegistry<PaymentsRegistry>();
            });
        }
    }
}
