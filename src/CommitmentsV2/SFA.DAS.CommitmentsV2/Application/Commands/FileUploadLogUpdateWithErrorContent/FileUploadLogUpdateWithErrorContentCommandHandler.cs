﻿using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Data;

namespace SFA.DAS.CommitmentsV2.Application.Commands.FileUploadLogUpdateWithErrorContent;

public class FileUploadLogUpdateWithErrorContentCommandHandler : IRequestHandler<FileUploadLogUpdateWithErrorContentCommand>
{
    private readonly Lazy<ProviderCommitmentsDbContext> _dbContext;
    private readonly ILogger<FileUploadLogUpdateWithErrorContentCommandHandler> _logger;

    public FileUploadLogUpdateWithErrorContentCommandHandler(Lazy<ProviderCommitmentsDbContext> dbContext, ILogger<FileUploadLogUpdateWithErrorContentCommandHandler> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task Handle(FileUploadLogUpdateWithErrorContentCommand command, CancellationToken cancellationToken)
    {
        var log = await _dbContext.Value.FileUploadLogs.FirstOrDefaultAsync(x=>x.Id.Equals(command.LogId), cancellationToken: cancellationToken);

        if (log == null)
        {
            _logger.LogError("FileUploadLog {logId} not found", command.LogId);
            throw new InvalidOperationException($"No FileLogUpload entry found for Id {command.LogId}");
        }

        if (log.ProviderId != command.ProviderId)
        {
            _logger.LogError("FileUploadLog {logId} doesn't belong to provider {providerId}", command.LogId, command.ProviderId);
            throw new InvalidOperationException($"Incorrect Provider {command.ProviderId} specified for FileUpload Id {command.LogId}");
        }

        log.Error = command.ErrorContent;
    }
}