CREATE TABLE [dbo].[Message]
(
	[Id] BIGINT NOT NULL PRIMARY KEY IDENTITY, 
    [CommitmentId] BIGINT NOT NULL, 
    [Text] NVARCHAR(MAX) NOT NULL, 
    [CreatedDateTime] DATETIME NOT NULL, 
    [Author] NVARCHAR(255) NOT NULL, 
    [CreatedBy] TINYINT NOT NULL, 
    CONSTRAINT [FK_Message_Commitment] FOREIGN KEY ([CommitmentId]) REFERENCES [Commitment]([Id])
)
GO

CREATE NONCLUSTERED INDEX [IX_Message_CommitmentId] ON [dbo].[Message] ([CommitmentId]) INCLUDE ([Author], [CreatedBy], [CreatedDateTime], [Text]) WITH (ONLINE = ON)