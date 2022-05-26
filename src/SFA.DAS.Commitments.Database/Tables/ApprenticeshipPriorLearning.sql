CREATE TABLE [dbo].ApprenticeshipPriorLearning
(
	[ApprenticeshipId] BIGINT NOT NULL,
	[ReducedDurationBy] INT NULL,
	[ReducedPriceBy] INT NULL,
    CONSTRAINT [PK_ApprenticeshipRecognisePriorLearning] PRIMARY KEY ([ApprenticeshipId]),
	CONSTRAINT [FK_ApprenticeshipRecognisePriorLearning_ApprenticeshipId] FOREIGN KEY ([ApprenticeshipId]) REFERENCES [Apprenticeship]([Id]) ON DELETE CASCADE
)
GO

