CREATE TABLE [dbo].[CommitmentHistory]
(
	[Id] BIGINT NOT NULL PRIMARY KEY IDENTITY, 
    [CommitmentId] BIGINT NOT NULL, 
    [UserId] NVARCHAR(50) NOT NULL, 
    [UpdatedByRole] NVARCHAR(50) NOT NULL, 
    [ChangeType] NVARCHAR(50) NOT NULL,
    [CreatedOn] DATETIME NULL, 
	[UpdatedByName] NVARCHAR(255) NULL
)