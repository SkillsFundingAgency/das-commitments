namespace SFA.DAS.ProviderApprenticeshipsService.Web.Models
{
    public class SignInUserModel
    {
        public string UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string UserSelected { get; set; }

        public long ProviderId { get; set; }
    }
}