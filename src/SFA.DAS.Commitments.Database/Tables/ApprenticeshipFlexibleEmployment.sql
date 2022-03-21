CREATE TABLE [dbo].ApprenticeshipFlexibleEmployment
(
	[ApprenticeshipId] BIGINT NOT NULL,
	[EmploymentPrice] INT NULL,
	[EmploymentEndDate] DATETIME2 NULL,
    CONSTRAINT [PK_FlexibleEmployment] PRIMARY KEY ([ApprenticeshipId])
)
GO

