using System;

namespace SFA.DAS.CommitmentsV2.Extensions
{
    public static class StringExtensions
    {
        public static T ToEnum<T>(this string value) where T : struct
        {
            return (T)Enum.Parse(typeof(T), value);
        }

        public static bool IsAValidEmailAddress(this string emailAsString)
        {
            try
            {
                var email = new System.Net.Mail.MailAddress(emailAsString);

                // check it contains a top level domain
                var parts = email.Address.Split('@');
                if (!ContainsTopLevelDomain(parts[1])) return false;
                if (!IsValidDomain(parts[1])) return false;

                return email.Address == emailAsString;
            }
            catch
            {
                return false;
            }
        }

        private static bool ContainsTopLevelDomain(string domainName)
        {
            if (!domainName.Contains(".") || domainName.EndsWith("."))
            {
                return false;
            }

            return true;
        }

        private static bool IsValidDomain(string domainName)
        {
            var check = Uri.CheckHostName(domainName);

            if (check == UriHostNameType.Dns)
            {
                return true;
            }

            return false;
        }
    }
}