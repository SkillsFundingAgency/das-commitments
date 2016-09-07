CREATE TABLE [dbo].[Tasks]
(
	[Id] BIGINT NOT NULL PRIMARY KEY IDENTITY, 
    [Assignee] VARCHAR(50) NOT NULL, 
    [TaskTemplateId] BIGINT NOT NULL, 
    [Name] VARCHAR(50) NOT NULL, 
    [Body] VARCHAR(MAX) NULL, 
    [TaskStatus] SMALLINT NOT NULL DEFAULT 0,
    [CreatedOn] DATETIME NOT NULL, 
    [CompletedOn] DATETIME NULL, 
    [CompletedBy] VARCHAR(50) NULL, 
    CONSTRAINT [FK_Tasks_TaskTemplates] FOREIGN KEY ([TaskTemplateId]) REFERENCES [TaskTemplates]([Id]), 
)
