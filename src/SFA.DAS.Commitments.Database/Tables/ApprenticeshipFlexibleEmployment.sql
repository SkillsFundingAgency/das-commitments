CREATE TABLE [dbo].ApprenticeshipFlexibleEmployment
(
	[ApprenticeshipId] BIGINT NOT NULL,
	[EmploymentPrice] INT NOT NULL,
	[EmploymentEndDate] DATETIME2 NOT NULL,
    CONSTRAINT [PK_FlexibleEmployment] PRIMARY KEY ([ApprenticeshipId])
)
GO

