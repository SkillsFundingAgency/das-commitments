﻿using SFA.DAS.CommitmentsV2.Api.Types.Responses;

namespace SFA.DAS.CommitmentsV2.Application.Commands.AddFileUploadLog;

public class AddFileUploadLogCommand : IRequest<BulkUploadAddLogResponse>
{
    public long? ProviderId { get; set; }
    public string FileName { get; set; }
    public int? RplCount { get; set; }
    public int? RowCount { get; set; }
    public string FileContent { get; set; }
}