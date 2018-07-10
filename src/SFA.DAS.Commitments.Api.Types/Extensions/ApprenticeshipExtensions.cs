
namespace SFA.DAS.Commitments.Api.Types.Extensions
{
    public static class ApprenticeshipExtensions
    {
        public static bool IsTranferFunded(this Apprenticeship.Apprenticeship apprenticeship)
        {
            return apprenticeship.TransferSenderId.HasValue;
        }
    }
}
