CREATE TABLE [dbo].ApprenticeshipPriorLearning
(
	[ApprenticeshipId] BIGINT NOT NULL,
	[DurationReducedBy] INT NULL,
	[PriceReducedBy] INT NULL,
	[IsAccelerated] AS (CASE WHEN [DurationReducedBy] >= 12 THEN CONVERT([BIT], 1) WHEN DurationReducedBy IS NULL THEN NULL ELSE CONVERT([BIT], 0) END), 
	[DurationReducedByHours] [decimal](18, 2) NULL,
	[WeightageReducedBy] [decimal](18, 2) NULL,
	[Qualification] [nchar](1000) NULL,
	[Reason] [text] NULL,
	[UpdatedDate] [datetime] NULL,
    CONSTRAINT [PK_ApprenticeshipRecognisePriorLearning] PRIMARY KEY ([ApprenticeshipId]),
	CONSTRAINT [FK_ApprenticeshipRecognisePriorLearning_ApprenticeshipId] FOREIGN KEY ([ApprenticeshipId]) REFERENCES [Apprenticeship]([Id]) ON DELETE CASCADE
)
GO