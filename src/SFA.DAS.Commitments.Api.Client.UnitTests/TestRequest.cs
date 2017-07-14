using System;

namespace SFA.DAS.Commitments.Api.Client.UnitTests
{
    public class TestRequest
    {
        public TestRequest(Uri uri, string requestContent)
        {
            Uri = uri;
            RequestContent = requestContent;
        }

        public Uri Uri { get; private set; }
        public string RequestContent { get; private set; }

        public override int GetHashCode()
        {
            return Uri.ToString().GetHashCode() ^ RequestContent.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            TestRequest otherRequest;
            otherRequest = (TestRequest)obj;

            return (obj.GetHashCode() == otherRequest.GetHashCode());
        }
    }
}