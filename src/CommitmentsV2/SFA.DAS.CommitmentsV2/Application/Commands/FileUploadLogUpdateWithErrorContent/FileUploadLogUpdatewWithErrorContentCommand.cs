using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Application.Commands.FileUploadLogUpdateWithErrorContent;

public class FileUploadLogUpdateWithErrorContentCommand : IRequest
{
    public long ProviderId { get; set; }
    public long LogId { get; set; }
    public string ErrorContent { get; set; }
    public UserInfo UserInfo { get; set; }
}