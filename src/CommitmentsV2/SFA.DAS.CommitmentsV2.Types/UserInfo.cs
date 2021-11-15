﻿namespace SFA.DAS.CommitmentsV2.Types
{
    public class UserInfo
    {
        public string UserId { get; set; }
        public string UserDisplayName { get; set; }
        public string UserEmail { get; set; }

        public static UserInfo System => new UserInfo {UserId = string.Empty, UserDisplayName = string.Empty, UserEmail = string.Empty};
    }
}