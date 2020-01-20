using System;

namespace SFA.DAS.CommitmentsV2.Types
{
    public class Message
    {
        public Message(string text, DateTime sentOn)
        {
            Text = text;
            SentOn = sentOn;
        }
        public string Text { get; set; }
        public DateTime SentOn { get; set; }
    }
}