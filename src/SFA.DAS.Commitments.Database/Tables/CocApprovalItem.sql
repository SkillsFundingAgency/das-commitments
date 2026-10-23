CREATE TABLE [dbo].[CocApprovalItem]
(
    [Id] UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),							
	[Created] DATETIME2 NOT NULL DEFAULT GETDATE(),
	[Updated] DATETIME2 NULL,
	[LearningKey] UNIQUEIDENTIFIER NOT NULL,		
    [ApprenticeshipId] BIGINT NOT NULL,	
	[LearningType] TINYINT NOT NULL,
	[UKPRN] [nvarchar](8) NOT NULL,
	[ULN] [nvarchar](10) NOT NULL,
	[Field] [nvarchar](100) NOT NULL,
	[Old] [nvarchar](MAX) NOT NULL,
	[New] [nvarchar](MAX) NOT NULL,
	[Status] TINYINT NULL,						
    [Reason] NVARCHAR(1000) NULL,
	[ProviderAcknowledgedAt] DATETIME2 NULL,
	[ProviderAcknowledgedBy] NVARCHAR(255) NULL,
	[EmployerAcknowledgedAt] DATETIME2 NULL,
	[EmployerAcknowledgedBy] NVARCHAR(255) NULL,
    CONSTRAINT [PK_CocApprovalFieldRequest] PRIMARY KEY CLUSTERED ([Id] ASC),
	INDEX [IX_CocApprovalFieldRequest_ApprenticeshipId] NONCLUSTERED ([ApprenticeshipId] ASC),
	INDEX [IX_CocApprovalFieldRequest_LearningKey] NONCLUSTERED ([LearningKey] ASC)
)