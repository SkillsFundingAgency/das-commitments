namespace SFA.DAS.Commitments.Domain.Entities
{
    public class ProviderUser
    {
        public long Ukprn { get; set; }

        public string FamilyName { get; set; }

        public string GivenName { get; set; }

        public string Email { get; set; }

        public string Title { get; set; }

        public bool ReceiveNotifications { get; set; }
    }
}