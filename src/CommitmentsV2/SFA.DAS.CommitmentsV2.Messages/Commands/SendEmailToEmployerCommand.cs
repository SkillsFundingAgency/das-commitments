using System.Collections.Generic;

namespace SFA.DAS.CommitmentsV2.Messages.Commands
{
    public class SendEmailToEmployerCommand
    {
        public long AccountId { get; }
        public string Template { get; }
        public Dictionary<string, string> Tokens { get; }
        public string EmailAddress { get; }
        public string NameToken { get; }

        public SendEmailToEmployerCommand(long accountId, string template, Dictionary<string, string> tokens, string emailAddress = null, string nameToken = null)
        {
            AccountId = accountId;
            Template = template;
            Tokens = tokens;
            EmailAddress = emailAddress;
            NameToken = nameToken;
        }
    }
}