using System.Linq;
using SFA.DAS.CommitmentsV2.Configuration;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;

namespace SFA.DAS.CommitmentsV2.Services
{
    public class EmailOptionalService : IEmailOptionalService
    {
        private readonly EmailOptionalConfiguration _config;

        public EmailOptionalService(EmailOptionalConfiguration config)
        {
            if (config == null)
            {
                _config = new EmailOptionalConfiguration {EmailOptionalEmployers = new long[0], EmailOptionalProviders  = new long[0]};
            }
            else
            {
                _config = config;
            }
        }

        public bool ApprenticeEmailIsOptionalFor(long employerId, long providerId)        
            => _config.EmailOptionalEmployers.Any(x => x == employerId) || _config.EmailOptionalProviders.Any(x => x == providerId);

        public bool ApprenticeEmailIsOptionalForEmployer(long employerId)
            => _config.EmailOptionalEmployers.Any(x => x == employerId);

        public bool ApprenticeEmailIsOptionalForProvider(long providerId)
            => _config.EmailOptionalProviders.Any(x => x == providerId);

        public bool ApprenticeEmailIsRequiredFor(long employerId, long providerId)
            => !ApprenticeEmailIsOptionalFor(employerId, providerId);

        public bool ApprenticeEmailIsRequiredForEmployer(long employerId)
            => !ApprenticeEmailIsOptionalForEmployer(employerId);

        public bool ApprenticeEmailIsRequiredForProvider(long providerId)
            => !ApprenticeEmailIsOptionalForProvider(providerId);
    }
}
