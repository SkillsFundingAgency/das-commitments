using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Api.Types.Requests
{
    public abstract class SaveDataRequest
    {
        public UserInfo UserInfo { get; set; }
    }
}