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
    [TrainingCourseVersion] NVARCHAR(10) NULL,
    [TrainingCourseVersionConfirmed] BIT NOT NULL DEFAULT 0,
    [TrainingCourseOption] NVARCHAR(126) NULL,
    [StandardUId] NVARCHAR(20) NULL,
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
    [EPAOrgId] CHAR(7) NULL,
    [CloneOf] BIGINT NULL,
    [ReservationId] UNIQUEIDENTIFIER NULL,
    [IsApproved] AS (CASE WHEN [PaymentStatus] > (0) THEN CONVERT([BIT], (1)) ELSE CONVERT([BIT], (0)) END) PERSISTED, 
    [CompletionDate] DATETIME NULL,
	[ContinuationOfId] BIGINT NULL,
	[MadeRedundant] BIT NULL, 
	[OriginalStartDate] DATETIME NULL
    CONSTRAINT [FK_Apprenticeship_Commitment] FOREIGN KEY ([CommitmentId]) REFERENCES [Commitment]([Id]),	  
    [Email] NVARCHAR(200) NULL, 
    CONSTRAINT [FK_Apprenticeship_AssessmentOrganisation] FOREIGN KEY ([EPAOrgId]) REFERENCES [AssessmentOrganisation]([EPAOrgId])
)
GO
CREATE NONCLUSTERED INDEX [IX_Apprenticeship_CommitmentId] ON [dbo].[Apprenticeship] ([CommitmentId]) INCLUDE ([AgreedOn], [AgreementStatus], [Cost], [CreatedOn], [DateOfBirth], [EmployerRef], [EndDate], [FirstName], [LastName], [NINumber], [PaymentOrder], [PaymentStatus], [ProviderRef], [StartDate], [TrainingCode], [TrainingName], [TrainingType], [ULN], [StopDate], [PauseDate], [HasHadDataLockSuccess], [PendingUpdateOriginator]) WITH (ONLINE = ON)
GO
CREATE NONCLUSTERED INDEX [IX_Apprenticeship_Uln_Statuses] ON [dbo].[Apprenticeship] ([ULN], [AgreementStatus], [PaymentStatus])
GO
CREATE NONCLUSTERED INDEX [IX_Apprenticeship_AgreedOn] ON [dbo].[Apprenticeship] ([AgreedOn]) INCLUDE ([CommitmentId], [PaymentStatus]) WITH (ONLINE = ON)
GO

CREATE UNIQUE NONCLUSTERED INDEX [UK_Apprenticeship_ReservationId] ON [dbo].[Apprenticeship] ([ReservationId] ASC) WHERE [ReservationId] IS NOT NULL
GO
CREATE NONCLUSTERED INDEX [IX_Apprenticeship_LastName] ON [dbo].[Apprenticeship] ([LastName]) WITH (ONLINE = ON)
GO

CREATE NONCLUSTERED INDEX [IX_Apprenticeship_CommitmentId2] ON [dbo].[Apprenticeship] ([CommitmentId])
INCLUDE ([AgreementStatus], [Cost], [DateOfBirth], [EmployerRef], [EndDate], [FirstName], [HasHadDataLockSuccess], [LastName], [NINumber], [PauseDate], [PaymentStatus], [PendingUpdateOriginator], [ProviderRef], [ReservationId], [StartDate], [StopDate], [TrainingCode], [TrainingName], [TrainingType], [ULN])
GO

CREATE NONCLUSTERED INDEX [IX_Apprenticeship_IsApprovedTrainingName_Filter] ON [dbo].[Apprenticeship] ([IsApproved]) INCLUDE ([CommitmentId],[TrainingName]) WITH (ONLINE = ON)
GO
CREATE NONCLUSTERED INDEX [IX_Apprenticeship_IsApprovedStartDate_Filter] ON [dbo].[Apprenticeship] ([IsApproved],[StartDate]) INCLUDE ([CommitmentId]) WITH (ONLINE=ON)
GO
CREATE NONCLUSTERED INDEX [IX_Apprenticeship_IsApprovedEndDate_Filter] ON [dbo].[Apprenticeship] ([IsApproved],[EndDate]) INCLUDE ([CommitmentId]) WITH (ONLINE=ON)
GO
CREATE NONCLUSTERED INDEX [IDX_Apprenticeship_ApprovedNameSearch] ON [dbo].[Apprenticeship] ([IsApproved]) INCLUDE ([CommitmentId],[FirstName],[LastName])
GO
CREATE NONCLUSTERED INDEX [IDX_Apprenticeship_ApprovedContinuationOf] ON [dbo].[Apprenticeship] ([IsApproved], [ContinuationOfId]) INCLUDE ([CommitmentId])
GO
CREATE NONCLUSTERED INDEX [IDX_Apprenticeship_Email] ON [dbo].[Apprenticeship] ([Email])
GO