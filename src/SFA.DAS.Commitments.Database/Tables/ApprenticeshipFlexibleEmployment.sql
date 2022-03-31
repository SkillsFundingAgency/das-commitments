CREATE TABLE [dbo].ApprenticeshipFlexibleEmployment
(
	[ApprenticeshipId] BIGINT NOT NULL,
	[EmploymentPrice] INT NULL,
	[EmploymentEndDate] DATETIME2 NULL,
	CONSTRAINT [FK_ApprenticeshipFlexibleEmployment_ApprenticeshipId] FOREIGN KEY ([ApprenticeshipId]) REFERENCES [Apprenticeship]([Id]),
    CONSTRAINT [PK_ApprenticeshipFlexibleEmployment] PRIMARY KEY ([ApprenticeshipId])
)
GO

