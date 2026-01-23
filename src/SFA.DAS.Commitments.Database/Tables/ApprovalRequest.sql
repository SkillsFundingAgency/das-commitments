CREATE TABLE [dbo].[ApprovalRequest]
(
    [Id] UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),							
	[Created] DATETIME2 NOT NULL DEFAULT GETDATE(),
	[Updated] DATETIME2 NULL,
    [LearningKey] UNIQUEIDENTIFIER NOT NULL,		
    [ApprenticeshipId] BIGINT NOT NULL,	
	[LearningType] TINYINT NOT NULL,
	[Status] TINYINT NULL,						
	[UKPRN] [nvarchar](8) NOT NULL,
	[ULN] [nvarchar](10) NOT NULL,
    CONSTRAINT [PK_ApprovalRequest] PRIMARY KEY CLUSTERED ([Id] ASC),
)

