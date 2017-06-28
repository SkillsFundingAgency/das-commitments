namespace SFA.DAS.Commitments.Infrastructure.Configuration
{
    public class ProviderUserApiConfiguration
    {
        public string ProviderTestEmail { get; set; }

        public int DasUserRoleId { get; set; }

        public string IdamsListUsersUrl { get; set; }

        public string ClientToken { get; set; }
    }
}