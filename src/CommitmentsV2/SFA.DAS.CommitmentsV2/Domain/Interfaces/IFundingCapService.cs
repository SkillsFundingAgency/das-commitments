using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Models;

namespace SFA.DAS.CommitmentsV2.Domain.Interfaces;

public interface IFundingCapService
{
    Task<IReadOnlyCollection<FundingCapCourseSummary>> FundingCourseSummary(IEnumerable<ApprenticeshipBase> list);
}