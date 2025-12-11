CREATE TABLE [dbo].[Change]
(
    [Id] BIGINT NOT NULL,							
	[ChangeTypeId] [tinyint] NOT NULL,
	[Old] [nvarchar](100) NULL,						
	[New] [nvarchar](100) NULL,
	[ChangeLogId] UNIQUEIDENTIFIER NOT NULL,
    CONSTRAINT [PK_Change] PRIMARY KEY CLUSTERED ([Id] ASC),
	CONSTRAINT [FK_Change_ChangeLog_ChangeLogId] FOREIGN KEY ([ChangeLogId]) REFERENCES [ChangeLog] ([Id]),
	CONSTRAINT [FK_Change_ChangeType_ChangeTypeId] FOREIGN KEY ([ChangeTypeId]) REFERENCES [ChangeType] ([Id]),
	INDEX [IX_Change_ChangeLogId] NONCLUSTERED ([ChangeLogId] ASC)
)

