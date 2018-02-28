-- Constrained to single row, see https://stackoverflow.com/questions/3967372/sql-server-how-to-constrain-a-table-to-contain-a-single-row

CREATE TABLE [dbo].[JobProgress]
(
    Lock char(1) NOT NULL DEFAULT 'X',
	[AddEpa_LastSubmissionEventId] [bigint] NULL,
	[IntTest_SchemaVersion] [int] NULL,
    constraint PK_JobProgress PRIMARY KEY (Lock),
    constraint CK_JobProgress_Locked CHECK (Lock='X')
)