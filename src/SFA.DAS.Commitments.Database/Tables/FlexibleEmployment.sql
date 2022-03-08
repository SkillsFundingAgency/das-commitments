CREATE TABLE [dbo].FlexibleEmployment
(
	[ApprenticeshipId] BIGINT NOT NULL,
	[EmploymentPrice] DECIMAL NOT NULL,
	[EmploymentEndDate] DATETIME2 NOT NULL,
    CONSTRAINT [PK_FlexibleEmployment] PRIMARY KEY ([ApprenticeshipId])
)
GO

