CREATE TABLE [dbo].[ApprenticeshipHistory]
(
	[Id] BIGINT NOT NULL PRIMARY KEY IDENTITY, 
    [ApprenticeshipId] BIGINT NOT NULL, 
    [UserId] NVARCHAR(50) NOT NULL, 
    [UpdatedByRole] SMALLINT NOT NULL, 
    [ChangeType] SMALLINT NOT NULL,
    [CreatedOn] DATETIME NOT NULL, 
)
