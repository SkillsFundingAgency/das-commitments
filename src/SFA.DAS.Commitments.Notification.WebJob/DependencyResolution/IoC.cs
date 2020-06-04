using StructureMap;

namespace SFA.DAS.Commitments.Notification.WebJob.DependencyResolution
{
    public static class IoC
    {
        public static IContainer Initialize()
        {
            return new Container(c =>
            {
                c.AddRegistry<NotificationsRegistry>();
                c.AddRegistry<DefaultRegistry>();
            });
        }
    }
}
