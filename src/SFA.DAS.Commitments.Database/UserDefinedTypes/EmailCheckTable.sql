CREATE TYPE [dbo].[EmailCheckTable] AS TABLE
(
	[RowId] BIGINT NOT NULL,
	[Email] NVARCHAR(200) NOT NULL,
	[StartDate] DATETIME2 NOT NULL,
	[EndDate] DATETIME2 NOT NULL,
	[ApprenticeshipId] BIGINT
)
