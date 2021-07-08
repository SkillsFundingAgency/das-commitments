CREATE TABLE [dbo].ApprenticeshipConfirmationStatus
(
	[ApprenticeshipId] BIGINT NOT NULL,
	[ApprenticeshipConfirmedOn] DATETIME2 NULL,
	[CommitmentsApprovedOn] DATETIME2 NOT NULL,
	[ConfirmationOverdueOn] DATETIME2 NULL,
	CONSTRAINT [FK_ApprenticeshipConfirmationStatus_ApprenticeshipId] FOREIGN KEY ([ApprenticeshipId]) REFERENCES [Apprenticeship]([Id]), 
    CONSTRAINT [PK_ApprenticeshipConfirmationStatus] PRIMARY KEY ([ApprenticeshipId])
)
GO

