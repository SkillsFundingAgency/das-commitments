namespace SFA.DAS.CommitmentsV2.Domain.Entities;

public class EmailToValidate
{
    public string Email { get; }
    public DateTime StartDate { get; }
    public DateTime EndDate { get; }
    public long? ApprenticeshipId { get; }

    public EmailToValidate(string email, DateTime startDate, DateTime endDate, long? apprenticeshipId)
    {
        Email = email;
        StartDate = startDate;
        EndDate = endDate;
        ApprenticeshipId = apprenticeshipId;
    }
}