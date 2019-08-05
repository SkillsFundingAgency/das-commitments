using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Mapping
{
    public static class EditStatusExtensions
    {
        public static Party ToParty(this EditStatus editStatus)
        {
            switch (editStatus)
            {
                case EditStatus.EmployerOnly :
                    return Party.Employer;
                case EditStatus.ProviderOnly:
                    return Party.Provider;
            }

            return Party.None;
        }
    }
}