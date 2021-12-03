CREATE TABLE [dbo].[TransferRequest]
(
	[Id] BIGINT NOT NULL PRIMARY KEY IDENTITY, 
    [CommitmentId] BIGINT NOT NULL,
	[TrainingCourses] NVARCHAR(MAX) NOT NULL,
	[Cost] MONEY NOT NULL,
	[Status] TINYINT NOT NULL,
	[TransferApprovalActionedByEmployerName] NVARCHAR(255),
	[TransferApprovalActionedByEmployerEmail] NVARCHAR(255),
	[TransferApprovalActionedOn] DATETIME2,

    [CreatedOn] DATETIME2 NOT NULL DEFAULT GETDATE(), 
    [FundingCap] MONEY NULL, 
    [AutoApproval] BIT NOT NULL DEFAULT 0, 
    CONSTRAINT [FK_TransferRequest_Commitment] FOREIGN KEY ([CommitmentId]) REFERENCES [Commitment]([Id])

)
GO

CREATE NONCLUSTERED INDEX [IX_TransferRequest_CommitmentId] ON [dbo].[TransferRequest] ([CommitmentId])
