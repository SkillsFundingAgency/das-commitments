CREATE TABLE [dbo].[FileUploadCohortLog]
(
    [Id] BIGINT NOT NULL PRIMARY KEY IDENTITY,
    [LogId] BIGINT NOT NULL,
    [CommitmentId] BIGINT NOT NULL,
    [RowCount] INT NULL,
	CONSTRAINT [FK_FileUploadCohortLog_FileUploadCohortLog] FOREIGN KEY([Id]) REFERENCES [dbo].[FileUploadCohortLog] ([Id]),
	CONSTRAINT [FK_FileUploadCohortLog_Commitment] FOREIGN KEY([CommitmentId]) REFERENCES [dbo].[Commitment] ([Id]),
)
GO