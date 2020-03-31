CREATE TABLE [dbo].[Accounts]
(                                           
    [Id] BIGINT NOT NULL,
    [HashedId] NCHAR(6) NOT NULL,
    [PublicHashedId] NCHAR(6) NOT NULL,
    [Name] NVARCHAR(100) NOT NULL,
    [Created] DATETIME2 NOT NULL,
    [Updated] DATETIME2 NULL,
    [LevyStatus] BIT NOT NULL DEFAULT 0,
    CONSTRAINT [PK_Accounts] PRIMARY KEY CLUSTERED ([Id] ASC)
)