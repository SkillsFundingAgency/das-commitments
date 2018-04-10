-- Constrained to single row, see https://stackoverflow.com/questions/3967372/sql-server-how-to-constrain-a-table-to-contain-a-single-row

CREATE TABLE [dbo].[JobProgress]
(
    Lock char(1) NOT NULL DEFAULT 'X',
	[AddEpa_LastSubmissionEventId] [bigint] NULL,
	[IntTest_SchemaVersion] [int] NULL, -- if we support adding on to existing data, how do we know what to delete when schema changes? use dedicated int test db? store range of ids? what about site data, test data, site data, generated data. per table as well, would need a new table to store ranges, e.g. integrationtestdata -> table, range start, range end. do we really want to do that?
	-- probably best to use own config & seperate int test db? could then add a table for this inttest stuff, and keep it out of the normal database (have script to create it, outside of database project?)
    constraint PK_JobProgress PRIMARY KEY (Lock),
    constraint CK_JobProgress_Locked CHECK (Lock='X')
)