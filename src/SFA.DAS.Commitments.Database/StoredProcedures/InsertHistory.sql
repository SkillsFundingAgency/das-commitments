CREATE PROCEDURE [dbo].[InsertHistory]
	@entityType NVARCHAR(50) = NULL,
	@entityId BIGINT = NULL, 
	@commitmentId BIGINT = NULL, 
	@apprenticeshipId BIGINT = NULL, 
    @userId NVARCHAR(50), 
    @updatedByRole NVARCHAR(50), 
    @changeType NVARCHAR(50),
    @updatedByName NVARCHAR(255), 
	@providerId BIGINT,
	@employerAccountId BIGINT,
    @originalState NVARCHAR(MAX), 
    @updatedState NVARCHAR(MAX),
	@createdOn DATETIME
AS

IF @entityType IS NOT NULL AND @entityId IS NOT NULL
BEGIN
	IF @entityType = 'Commitment'
	BEGIN
		SET @commitmentId = @entityId
		SET @apprenticeshipId = null
	END

	IF @entityType = 'Apprenticeship'
	BEGIN
		SET @commitmentId = null
		SET @apprenticeshipId = @entityId
	END
END

INSERT INTO [dbo].[History]
	(CommitmentId, ApprenticeshipId, UserId, UpdatedByRole, ChangeType, CreatedOn, ProviderId, EmployerAccountId, UpdatedByName, OriginalState, UpdatedState)
VALUES
	(@commitmentId, @apprenticeshipId, @userId, @updatedByRole, @changeType, @createdOn, @providerId, @employerAccountId, @updatedByName, @originalState, @updatedState)