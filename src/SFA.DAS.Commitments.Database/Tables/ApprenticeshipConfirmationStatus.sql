CREATE TABLE [dbo].ApprenticeshipConfirmationStatus
(
	[Id] BIGINT NOT NULL IDENTITY PRIMARY KEY,
	[ApprenticeshipId] BIGINT NOT NULL,
	[ApprenticeshipConfirmedOn] DATETIME2 NULL,
	[CommitmentsApprovedOn] DATETIME2 NULL,
	[ConfirmationOverdueOn] DATETIME2 NULL,
	CONSTRAINT [FK_ApprenticeshipConfirmationStatus_ApprenticeshipId] FOREIGN KEY ([ApprenticeshipId]) REFERENCES [Apprenticeship]([Id])
)
GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_ApprenticeshipConfirmationStatus_ApprenticeshipId] ON ApprenticeshipConfirmationStatus ([ApprenticeshipId]);
GO

