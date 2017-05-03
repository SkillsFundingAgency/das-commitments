CREATE PROCEDURE [dbo].[InsertHistory]
	@entityType NVARCHAR(50),
	@entityId BIGINT, 
    @userId NVARCHAR(50), 
    @updatedByRole NVARCHAR(50), 
    @changeType NVARCHAR(50),
    @updatedByName NVARCHAR(255), 
    @originalState NVARCHAR(MAX), 
    @updatedState NVARCHAR(MAX)
AS

INSERT INTO [dbo].[History]
	(EntityType, EntityId, UserId, UpdatedByRole, ChangeType, CreatedOn, UpdatedByName, OriginalState, UpdatedState)
VALUES
	(@entityType, @entityId, @userId, @updatedByRole, @changeType, GETDATE(), @updatedByName, @originalState, @updatedState)