CREATE PROCEDURE [dbo].[InsertHistory]
	@commitmentId BIGINT, 
	@apprenticeshipId BIGINT, 
    @userId NVARCHAR(50), 
    @updatedByRole NVARCHAR(50), 
    @changeType NVARCHAR(50),
    @updatedByName NVARCHAR(255), 
    @originalState NVARCHAR(MAX), 
    @updatedState NVARCHAR(MAX),
	@createdOn DATETIME
AS

INSERT INTO [dbo].[History]
	(CommitmentId, ApprenticeshipId, UserId, UpdatedByRole, ChangeType, CreatedOn, UpdatedByName, OriginalState, UpdatedState)
VALUES
	(@commitmentId, @apprenticeshipId, @userId, @updatedByRole, @changeType, @createdOn, @updatedByName, @originalState, @updatedState)