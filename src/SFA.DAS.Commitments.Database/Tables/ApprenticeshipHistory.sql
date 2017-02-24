CREATE TABLE [dbo].[ApprenticeshipHistory]
(
	[Id] BIGINT NOT NULL PRIMARY KEY IDENTITY, 
    [ApprenticeshipId] BIGINT NOT NULL, 
    [UserId] BIGINT NOT NULL, 
    [UpdatedByRole] SMALLINT NOT NULL, 
    [ChangeType] SMALLINT NOT NULL,
    [CreatedOn] DATETIME NOT NULL, 
)
