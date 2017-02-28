CREATE TABLE [dbo].[CommitmentHistory]
(
	[Id] BIGINT NOT NULL PRIMARY KEY IDENTITY, 
    [CommitmentId] BIGINT NOT NULL, 
    [UserId] NVARCHAR(50) NOT NULL, 
    [UpdatedByRole] SMALLINT NOT NULL, 
    [ChangeType] SMALLINT NOT NULL,
    [CreatedOn] DATETIME NULL, 
)