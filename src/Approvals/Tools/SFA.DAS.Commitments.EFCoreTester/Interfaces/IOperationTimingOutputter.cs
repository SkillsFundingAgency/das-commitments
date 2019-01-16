namespace SFA.DAS.Commitments.EFCoreTester.Interfaces
{
    public interface IOperationTimingOutputter
    {
        void ShowLog(IOperation operation);
        void ShowSummary(IOperation operation);
    }
}