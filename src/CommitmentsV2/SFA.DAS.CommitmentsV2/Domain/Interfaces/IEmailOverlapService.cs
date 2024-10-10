using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Models;

namespace SFA.DAS.CommitmentsV2.Domain.Interfaces;

public interface IEmailOverlapService
{
    Task<List<OverlappingEmail>> GetOverlappingEmails(EmailToValidate emailToValidate, long? cohortId, CancellationToken cancellationToken);
    Task<List<OverlappingEmail>> GetOverlappingEmails(long cohortId, CancellationToken cancellationToken);
}