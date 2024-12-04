using SFA.DAS.CommitmentsV2.Domain.Entities;

namespace SFA.DAS.CommitmentsV2.Models;

public class OverlappingEmail
{
    public long RowId { get; set; }
    public long? Id { get; set; }
    public long? CohortId { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsApproved { get; set; }
    public string Email {get; set; }
    public OverlapStatus OverlapStatus { get; set; }
}