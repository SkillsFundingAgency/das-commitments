using System.Collections.Generic;

namespace SFA.DAS.CommitmentsV2.Messages.Commands
{
    public class SendEmailCommand
    {
        public string TemplateId { get; }
        public string RecipientsAddress { get; }
        public string ReplyToAddress { get; }
        public IReadOnlyDictionary<string, string> Tokens { get; }

        public SendEmailCommand(string templateId, string recipientsAddress, string replyToAddress, IReadOnlyDictionary<string, string> tokens)
        {
            TemplateId = templateId;
            RecipientsAddress = recipientsAddress;
            ReplyToAddress = replyToAddress;
            Tokens = tokens;
        }
    }
}
