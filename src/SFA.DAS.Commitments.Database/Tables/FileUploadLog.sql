CREATE TABLE [dbo].[FileUploadLog]
(
    [Id] BIGINT NOT NULL PRIMARY KEY IDENTITY,
    [ProviderId] BIGINT NOT NULL,
    [FileName] NVARCHAR(100) NOT NULL,
    [RplCount] INT NULL,
    [RowCount] INT NULL,
    [ProviderAction] NVARCHAR(100) NOT NULL,  
    [FileContent] NVARCHAR(MAX) NOT NULL,    
    [CreatedOn] DateTime NOT NULL,
    [CompletedOn] DateTime NULL,
    [Error] NVARCHAR(MAX) NOT NULL
)