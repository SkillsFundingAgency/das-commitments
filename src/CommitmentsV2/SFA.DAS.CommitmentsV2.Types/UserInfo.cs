namespace SFA.DAS.CommitmentsV2.Types
{
    public class UserInfo
    {
        public string UserId { get; set; }
        public string UserDisplayName { get; set; }
        public string UserEmail { get; set; }

        public static UserInfo System => new UserInfo { UserId = string.Empty, UserDisplayName = string.Empty, UserEmail = string.Empty };
    }

    public static class UserInfoExtensions
    {
        public static bool IsSystem(this UserInfo userInfo)
        {
            return userInfo.UserId == string.Empty &&
                   userInfo.UserDisplayName == string.Empty &&
                   userInfo.UserEmail == string.Empty;
        }
    }
}