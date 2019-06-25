using System;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Domain.Extensions
{
    public static class PartyExtensions
    {
        public static EditStatus ToEditStatus(this Party party)
        {
            switch (party)
            {
                case Party.Employer:
                    return EditStatus.EmployerOnly;
                case Party.Provider:
                    return EditStatus.ProviderOnly;
                default:
                    throw new ArgumentException($"Unable to map Party {party} to EditStatus");
            }
        }

        public static Originator ToOriginator(this Party party)
        {
            switch (party)
            {
                case Party.Employer:
                    return Originator.Employer;
                case Party.Provider:
                    return Originator.Provider;
                default:
                    throw new ArgumentException($"Unable to map Party {party} to Originator");
            }
        }
    }
}