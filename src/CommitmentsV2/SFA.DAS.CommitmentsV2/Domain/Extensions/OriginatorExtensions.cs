using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Domain.Extensions
{
    public static class OriginatorExtensions
    {
        public static Party ToParty(this Originator originator)
        {
            switch (originator)
            {
                case Originator.Employer:
                    return Party.Employer;
                case Originator.Provider:
                    return Party.Provider;
                default:
                    throw new ArgumentException($"Unable to map Originator {originator} to Party");
            }
        }
    }
}