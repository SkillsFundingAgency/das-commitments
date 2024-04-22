using System;

namespace SFA.DAS.CommitmentsV2.Messages.Events;

public class ApprenticeshipStartDateChangedEvent
{
	public Guid ApprenticeshipKey { get; set; }

	public int ApprenticeshipId { get; set; }

	public DateTime ActualStartDate { get; set; }

	public int EmployerAccountId { get; set; }

	public int ProviderId { get; set; }

	public DateTime ApprovedDate { get; set; }

    public ChangeUser ProviderUser { get; set; } = new ChangeUser();
    public ChangeUser EmployerUser { get; set; } = new ChangeUser();

    public string Initiator { get; set; }
}

public class  ChangeUser
{
	public string UserId { get; set; }
	public string UserDisplayName { get; set; }
	public string UserEmail { get; set; }
}
