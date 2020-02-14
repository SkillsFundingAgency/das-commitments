using System.Collections.Generic;

namespace SFA.DAS.CommitmentsV2.Messages.Commands
{
    public class SendEmailToProviderCommand
    {
        public long ProviderId { get; }
        public string Template { get; }
        public Dictionary<string, string> Tokens { get; }
        public string EmailAddress { get; }

        public SendEmailToProviderCommand(long providerId, string template, Dictionary<string, string> tokens, string emailAddress = null)
        {
            ProviderId = providerId;
            Template = template;
            Tokens = tokens;
            EmailAddress = emailAddress;
        }
    }
}
