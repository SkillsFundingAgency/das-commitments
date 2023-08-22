CREATE TABLE [dbo].[FileUploadLog]
(
    [Id] BIGINT NOT NULL PRIMARY KEY IDENTITY,
    [ProviderUkprn] BIGINT NOT NULL,
    [FileName] VARCHAR(500) NOT NULL,
    [RplCount] INT NULL,
    [RowCount] INT NULL,
    [ProviderAction] VARCHAR(500) NOT NULL,  
    [FileContent] VARCHAR(MAX) NOT NULL,    
    [CreatedOn] DateTime NOT NULL,
    [CompletedOn] DateTime NULL,
    [Error] VARCHAR(MAX) NOT NULL
)