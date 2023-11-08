CREATE TABLE [dbo].[FileUploadLog]
(
    [Id] BIGINT NOT NULL PRIMARY KEY IDENTITY,
    [ProviderId] BIGINT NULL,
    [FileName] NVARCHAR(100) NULL,
    [RplCount] INT NULL,
    [RowCount] INT NULL,
    [ProviderAction] NVARCHAR(100) NULL,
    [FileContent] NVARCHAR(MAX) NULL,
    [CreatedOn] DATETIME2 NOT NULL DEFAULT GETDATE(), 
    [CompletedOn] DATETIME2 NULL,
    [Error] NVARCHAR(MAX) NULL
)