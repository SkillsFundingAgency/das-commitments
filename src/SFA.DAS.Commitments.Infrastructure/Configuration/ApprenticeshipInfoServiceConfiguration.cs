using SFA.DAS.Commitments.Domain.Entities.TrainingProgramme;

namespace SFA.DAS.Commitments.Infrastructure.Configuration
{
    public class ApprenticeshipInfoServiceConfiguration : IApprenticeshipInfoServiceConfiguration
    {
        public string BaseUrl { get; set; }
    }
}