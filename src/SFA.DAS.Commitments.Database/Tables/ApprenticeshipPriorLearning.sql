CREATE TABLE [dbo].ApprenticeshipPriorLearning
(
	[ApprenticeshipId] BIGINT NOT NULL,
	[DurationReducedBy] INT NULL,
	[PriceReducedBy] INT NULL,
	[IsAccelerated] AS (CASE WHEN [DurationReducedBy] >= 12 THEN CONVERT([BIT], 1) WHEN DurationReducedBy IS NULL THEN NULL ELSE CONVERT([BIT], 0) END), 
	[DurationReducedByHours] INT NULL,
	[WeightageReducedBy] INT NULL,
	[ReasonForRplReduction] [nvarchar](1000) NULL,
	[QualificationsForRplReduction] [text] NULL,
	[IsDurationReducedByRpl] BIT NULL,
    CONSTRAINT [PK_ApprenticeshipRecognisePriorLearning] PRIMARY KEY ([ApprenticeshipId]),
	CONSTRAINT [FK_ApprenticeshipRecognisePriorLearning_ApprenticeshipId] FOREIGN KEY ([ApprenticeshipId]) REFERENCES [Apprenticeship]([Id]) ON DELETE CASCADE
)
GO