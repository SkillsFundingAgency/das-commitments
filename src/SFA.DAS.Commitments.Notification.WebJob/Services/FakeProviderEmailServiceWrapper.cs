using System.Collections.Generic;
using System.Threading.Tasks;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Infrastructure.Data;
using SFA.DAS.Commitments.Notification.WebJob.Configuration;

namespace SFA.DAS.Commitments.Notification.WebJob.Services
{
    public class FakeProviderEmailServiceWrapper : IProviderEmailServiceWrapper
    {
        private readonly CommitmentNotificationConfiguration _config;

        public FakeProviderEmailServiceWrapper(CommitmentNotificationConfiguration config)
        {
            _config = config;
        }

        public async Task<List<ProviderUser>> GetUsersAsync(long ukprn)
        {
            return await Task.FromResult(
                new List<ProviderUser>
                {
                    new ProviderUser
                        {
                            Email = _config.TestUserEmail,
                            GivenName = "First name",
                            FamilyName = "Family Name",
                            Title = "Dr",
                            Ukprn = ukprn
                        }
                });
        }
    }
}
