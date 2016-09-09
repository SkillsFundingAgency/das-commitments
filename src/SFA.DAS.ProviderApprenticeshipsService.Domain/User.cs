namespace SFA.DAS.ProviderApprenticeshipsService.Domain
{
    public class User
    {
        public long Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string UserRef { get; set; }
        public long ProviderId { get; set; }
    }
}