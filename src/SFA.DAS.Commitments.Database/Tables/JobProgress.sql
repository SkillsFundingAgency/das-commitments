CREATE TABLE [dbo].[JobProgress]
(
    Lock char(1) NOT NULL DEFAULT 'X',
	[AddEpa_LastSubmissionEventId] [bigint] NULL,
    constraint PK_JobProgress PRIMARY KEY (Lock),
    constraint CK_JobProgress_Locked CHECK (Lock='X')
)
