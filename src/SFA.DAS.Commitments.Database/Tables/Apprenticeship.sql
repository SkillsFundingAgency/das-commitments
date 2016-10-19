CREATE TABLE [dbo].[Apprenticeship]
(
	[Id] BIGINT NOT NULL PRIMARY KEY IDENTITY, 
    [CommitmentId] BIGINT NOT NULL, 
    [FirstName] NVARCHAR(100) NULL, 
    [LastName] NVARCHAR(100) NULL, 
    [ULN] NVARCHAR(50) NULL, 
    [TrainingType] INT NULL, 
    [TrainingCode] NVARCHAR(20) NULL, 
    [TrainingName] NVARCHAR(126) NULL, 
    [Cost] DECIMAL NULL, 
    [StartDate] DATETIME NULL, 
    [EndDate] DATETIME NULL, 
    [Status] SMALLINT NOT NULL DEFAULT 0, 
    [AgreementStatus] SMALLINT NOT NULL DEFAULT 0, 
    CONSTRAINT [FK_Apprenticeship_Commitment] FOREIGN KEY ([CommitmentId]) REFERENCES [Commitment]([Id])
)
