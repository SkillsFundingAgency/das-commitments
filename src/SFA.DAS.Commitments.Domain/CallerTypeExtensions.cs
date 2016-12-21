namespace SFA.DAS.Commitments.Domain
{
    public static class CallerTypeExtensions
    {
        public static bool IsEmployer(this CallerType type)
        {
            return type == CallerType.Employer;
        }

        public static bool IsProvider(this CallerType type)
        {
            return type == CallerType.Provider;
        }
    }
}
