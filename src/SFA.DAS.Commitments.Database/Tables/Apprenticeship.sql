CREATE TABLE [dbo].[Apprenticeship]
(
	[Id] BIGINT NOT NULL PRIMARY KEY IDENTITY, 
    [CommitmentId] BIGINT NOT NULL, 
    [ApprenticeName] NVARCHAR(100) NULL, 
    [ULN] NVARCHAR(50) NULL, 
    [TrainingId] INT NULL, 
    [Cost] DECIMAL NULL, 
    [StartDate] DATETIME NULL, 
    [EndDate] DATETIME NULL, 
    [Status] SMALLINT NOT NULL DEFAULT 0, 
    [AgreementStatus] SMALLINT NOT NULL DEFAULT 0, 
    CONSTRAINT [FK_Apprenticeship_Commitment] FOREIGN KEY ([CommitmentId]) REFERENCES [Commitment]([Id])
)
