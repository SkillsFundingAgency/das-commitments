CREATE TABLE [dbo].[ApprovalFieldRequest]
(
    [Id] UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),							
	[Created] DATETIME2 NOT NULL DEFAULT GETDATE(),
	[Field] [nvarchar](100) NOT NULL,
	[Old] [nvarchar](255) NOT NULL,
	[New] [nvarchar](255) NOT NULL,
    [ApprovalRequestId] UNIQUEIDENTIFIER NOT NULL,							
	[Status] TINYINT NULL,						
	[ApproverId] [nvarchar](100) NULL,
	[Reason] [nvarchar](1000) NULL,
	[Updated] DATETIME2 NULL,
    CONSTRAINT [PK_ApprovalFieldRequest] PRIMARY KEY CLUSTERED ([Id] ASC),
	CONSTRAINT [FK_ApprovalFieldRequest_ApprovalRequestId] FOREIGN KEY ([ApprovalRequestId]) REFERENCES [ApprovalRequest] ([Id]),
	INDEX [IX_ApprovalRequest_ApprovalRequestId] NONCLUSTERED ([ApprovalRequestId] ASC)


)

