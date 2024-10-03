namespace SFA.DAS.CommitmentsV2.Application.Queries.GetProviderCommitmentAgreements;

public class GetProviderCommitmentAgreementQuery : IRequest<GetProviderCommitmentAgreementResult>
{
    public long ProviderId { get; }

    public GetProviderCommitmentAgreementQuery(long providerId)
    {
        ProviderId = providerId;
    }
}