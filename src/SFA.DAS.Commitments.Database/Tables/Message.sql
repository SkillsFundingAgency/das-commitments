CREATE TABLE [dbo].[Message]
(
	[Id] BIGINT NOT NULL PRIMARY KEY IDENTITY, 
    [CommitmentId] BIGINT NOT NULL, 
    [Message] NVARCHAR(MAX) NOT NULL, 
    [CreatedOn] DATETIME NOT NULL, 
    [Author] NVARCHAR(255) NOT NULL, 
    [Creator] TINYINT NOT NULL, 
    CONSTRAINT [FK_Message_Commitment] FOREIGN KEY ([CommitmentId]) REFERENCES [Commitment]([Id])
)
GO

CREATE NONCLUSTERED INDEX [IX_Message_CommitmentId] ON [dbo].[Apprenticeship] ([CommitmentId])