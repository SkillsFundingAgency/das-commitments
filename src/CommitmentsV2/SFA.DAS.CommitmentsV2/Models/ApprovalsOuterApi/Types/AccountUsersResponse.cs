namespace SFA.DAS.CommitmentsV2.Models.ApprovalsOuterApi.Types;

public class AccountUsersResponse : List<TeamMember>
{
    public string HashedAccountId { get; set; }
}

public class TeamMember
{
    public string UserRef { get; set; }

    public string Name { get; set; }

    public string Email { get; set; }

    public string Role { get; set; }

    public bool CanReceiveNotifications { get; set; }

    public string Status { get; set; }
}