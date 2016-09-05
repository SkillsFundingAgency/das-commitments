CREATE TABLE [dbo].[TaskAlerts]
(
	[Id] BIGINT NOT NULL PRIMARY KEY IDENTITY, 
    [TaskId] BIGINT NOT NULL, 
    [UserId] VARCHAR(50) NOT NULL, 
    [CreatedOn] DATETIME NOT NULL, 
    CONSTRAINT [FK_TaskAlerts_Tasks] FOREIGN KEY ([TaskId]) REFERENCES [Tasks]([Id]), 

)
