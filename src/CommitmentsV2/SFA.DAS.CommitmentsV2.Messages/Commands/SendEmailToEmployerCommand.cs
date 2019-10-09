using System.Collections.Generic;

namespace SFA.DAS.CommitmentsV2.Messages.Commands
{
    public class SendEmailToEmployerCommand
    {
        public long AccountId { get; }
        public string Template { get; }
        public IReadOnlyDictionary<string, string> Tokens { get; }
        public string EmailAddress { get; }

        public SendEmailToEmployerCommand(long accountId, string template, IReadOnlyDictionary<string, string> tokens, string emailAddress = null)
        {
            AccountId = accountId;
            Template = template;
            Tokens = tokens;
            EmailAddress = emailAddress;
        }
    }
}