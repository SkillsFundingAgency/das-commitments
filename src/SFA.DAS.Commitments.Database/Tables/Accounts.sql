CREATE TABLE [dbo].[Accounts]
(                                           
    [Id] BIGINT NOT NULL,
    [HashedId] CHAR(6) NOT NULL,
    [PublicHashedId] CHAR(6) NOT NULL,
    [Name] NVARCHAR(100) NOT NULL,
    [Created] DATETIME2 NOT NULL,
    [Updated] DATETIME2 NULL,
    CONSTRAINT [PK_Accounts] PRIMARY KEY CLUSTERED ([Id] ASC)
)