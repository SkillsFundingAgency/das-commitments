using SFA.DAS.Commitments.AcademicYearEndProcessor.WebJob.Configuration;
using SFA.DAS.Messaging.AzureServiceBus;
using SFA.DAS.Messaging.AzureServiceBus.StructureMap;
using SFA.DAS.NLog.Logger;
using StructureMap;

namespace SFA.DAS.Commitments.AcademicYearEndProcessor.WebJob.DependencyResolution
{
    public static class IoC
    {
        private const string ServiceName = "SFA.DAS.CommitmentsAcademicYearEndProcessor";
        private const string ServiceVersion = "1.0";

        public static IContainer Initialize()
        {
            return new Container(c =>
            {
                c.AddRegistry<DefaultRegistry>();
                c.Policies.Add(new TopicMessagePublisherPolicy<CommitmentsAcademicYearEndProcessorConfiguration>(ServiceName, ServiceVersion, new NLogLogger(typeof(TopicMessagePublisher))));
            });
        }
    }
}
