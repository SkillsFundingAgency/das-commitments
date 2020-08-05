namespace SFA.DAS.Commitments.Application.Commands.UpdateApprenticeshipStatus
{
    public sealed class StopApprenticeshipCommand : ApprenticeshipStatusChangeCommand
    {
        public bool? MadeRedundant { get; set; }
    }
}