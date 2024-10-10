namespace SFA.DAS.CommitmentsV2.Api.Types.Requests;

public class FileUploadLogUpdateWithErrorContentRequest : SaveDataRequest
{
    public string ErrorContent { get; set; }
}