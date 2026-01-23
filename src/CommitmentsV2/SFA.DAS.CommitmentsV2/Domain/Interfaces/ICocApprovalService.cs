namespace SFA.DAS.CommitmentsV2.Domain.Interfaces;

public interface ICocApprovalService
{
    Task<bool> AutoApproveOrSomethingElse();
}