CREATE TABLE [dbo].[FileUploadCohortLog]
(
    [Id] BIGINT NOT NULL PRIMARY KEY IDENTITY,
    [FileUploadLogId] BIGINT NOT NULL,
    [CommitmentId] BIGINT NOT NULL,
    [RowCount] INT NULL,
	CONSTRAINT [FK_FileUploadCohortLog_FileUploadLog] FOREIGN KEY([FileUploadLogId]) REFERENCES [dbo].[FileUploadLog] ([Id]),
	CONSTRAINT [FK_FileUploadCohortLog_Commitment] FOREIGN KEY([CommitmentId]) REFERENCES [dbo].[Commitment] ([Id]),
)
GO
CREATE NONCLUSTERED INDEX [IDX_FileUploadCohortLog_FileUploadLogId] ON [dbo].[FileUploadCohortLog] ([FileUploadLogId])
GO