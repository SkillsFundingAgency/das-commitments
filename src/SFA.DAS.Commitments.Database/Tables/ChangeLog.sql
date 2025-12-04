CREATE TABLE [dbo].[ChangeLog]
(
    [Id] UNIQUEIDENTIFIER NOT NULL,                             
    [CreatedOn] DATETIME2 NOT NULL DEFAULT (GETDATE()),         
	[Status] [tinyint] NOT NULL,                                
    [LearningKey] UNIQUEIDENTIFIER NOT NULL,                    
    [UKPRN] BIGINT NOT NULL,                                    
    [AgreementId] NVARCHAR(6) NOT NULL,                         
    [ApprovalUserId] UNIQUEIDENTIFIER NULL,                     
    CONSTRAINT [PK_ChangeLog] PRIMARY KEY CLUSTERED ([Id] ASC),
)
