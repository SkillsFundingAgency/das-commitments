﻿CREATE TABLE [dbo].[History]
(
	[Id] BIGINT NOT NULL PRIMARY KEY IDENTITY,
	[EntityType] NVARCHAR(50) NOT NULL,
	[EntityId] BIGINT NOT NULL, 
    [UserId] NVARCHAR(50) NOT NULL, 
    [UpdatedByRole] NVARCHAR(50) NOT NULL, 
    [ChangeType] NVARCHAR(50) NOT NULL,
    [CreatedOn] DATETIME NOT NULL, 
    [UpdatedByName] NVARCHAR(255) NULL, 
    [OriginalState] NVARCHAR(MAX) NULL, 
    [UpdatedState] NVARCHAR(MAX) NULL
)
