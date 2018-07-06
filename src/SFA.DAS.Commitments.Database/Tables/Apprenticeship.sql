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
    [AgreementStatus] SMALLINT NOT NULL DEFAULT 0, 
    [PaymentStatus] SMALLINT NOT NULL DEFAULT 0, 
    [DateOfBirth] DATETIME NULL, 
    [NINumber] NVARCHAR(10) NULL, 
    [EmployerRef] NVARCHAR(50) NULL, 
    [ProviderRef] NVARCHAR(50) NULL, 
    [CreatedOn] DATETIME NULL, 
    [AgreedOn] DATETIME NULL, 
    [PaymentOrder] INT NULL, 
    [StopDate] DATE NULL, 
    [PauseDate] DATE NULL, 
	[HasHadDataLockSuccess] BIT NOT NULL DEFAULT 0,
	-- PendingUpdateOriginator is a combination of ApprenticeshipUpdate Originator and Status
	-- if not null, Status = Pending, contains PendingUpdateOriginator = Originator
	-- if null, no ApprenticeshipUpdate or Status != Pending
	-- (we could store Originator and Status instead)
	[PendingUpdateOriginator] TINYINT NULL,
	[EPAOrgId] CHAR(7) NULL
    CONSTRAINT [FK_Apprenticeship_Commitment] FOREIGN KEY ([CommitmentId]) REFERENCES [Commitment]([Id]),
	CONSTRAINT [FK_Apprenticeship_AssessmentOrganisation] FOREIGN KEY ([EPAOrgId]) REFERENCES [AssessmentOrganisation]([EPAOrgId])
)
GO

CREATE NONCLUSTERED INDEX [IX_Apprenticeship_CommitmentId] ON [dbo].[Apprenticeship] ([CommitmentId]) INCLUDE ([AgreedOn], [AgreementStatus], [Cost], [CreatedOn], [DateOfBirth], [EmployerRef], [EndDate], [FirstName], [LastName], [NINumber], [PaymentOrder], [PaymentStatus], [ProviderRef], [StartDate], [TrainingCode], [TrainingName], [TrainingType], [ULN], [StopDate], [PauseDate], [HasHadDataLockSuccess], [PendingUpdateOriginator]) WITH (ONLINE = ON)
GO
CREATE NONCLUSTERED INDEX [IX_Apprenticeship_Uln_Statuses] ON [dbo].[Apprenticeship] ([ULN], [AgreementStatus], [PaymentStatus])
GO
CREATE NONCLUSTERED INDEX [IX_Apprenticeship_AgreedOn] ON [dbo].[Apprenticeship] ([AgreedOn]) INCLUDE ([CommitmentId], [PaymentStatus]) WITH (ONLINE = ON)
GO
CREATE NONCLUSTERED INDEX [IX_Apprenticeship_Uln_PaymentStatus] ON [dbo].[Apprenticeship] ([PaymentStatus], [ULN]) INCLUDE ([AgreedOn], [CommitmentId], [StartDate], [StopDate]) WITH (ONLINE = ON)
GO
