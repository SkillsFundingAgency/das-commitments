using SFA.DAS.CommitmentsV2.Api.Types.Responses;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetAccountStatus;

public sealed class GetAccountStatusQueryResult
{
    public List<AccountStatusProviderCourse> Active { get; init; } = new();

    public List<AccountStatusProviderCourse> Completed { get; init; } = new();

    public List<AccountStatusProviderCourse> NewStart { get; init; } = new();
}