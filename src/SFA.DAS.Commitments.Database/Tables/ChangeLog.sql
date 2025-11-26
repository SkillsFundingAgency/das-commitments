CREATE TABLE [dbo].[ChangeLog]
(
    [Id] UNIQUEIDENTIFIER NOT NULL,                             -- Richard - this makes sense if this value is publicly exposed
    [CreatedOn] DATETIME2 NOT NULL DEFAULT (GETDATE()),         -- Richard - added default value and set type to DATETIME2 
	[Status] [tinyint] NOT NULL,                                -- Richard - Nothing in the HLD states what this value should be
    [LearningUri] UNIQUEIDENTIFIER NOT NULL,                    -- Richard - I assume this is a UNIQUEIDENTIFIER as per LearningRecord (for approved changes?)
    [UKPRN] BIGINT NOT NULL,                                    -- Richard - changed from varchar(100) to BIGINT
    [AgreementId] NVARCHAR(6) NOT NULL,                         -- Richard - changed from VARCHAR(10) to NVARCHAR(6). Should we be storing the AccountLegalEntityId here?
    [ApprovalUserId] UNIQUEIDENTIFIER NULL,                     -- Richard - does this record who approved or rejected the change?
    CONSTRAINT [PK_ChangeLog] PRIMARY KEY CLUSTERED ([Id] ASC),
)
