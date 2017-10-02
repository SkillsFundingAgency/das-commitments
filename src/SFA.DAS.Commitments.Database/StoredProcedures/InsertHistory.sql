CREATE PROCEDURE [dbo].[InsertHistory]
	@commitmentId BIGINT, 
	@apprenticeshipId BIGINT, 
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

INSERT INTO [dbo].[History]
	(CommitmentId, ApprenticeshipId, UserId, UpdatedByRole, ChangeType, CreatedOn, ProviderId, EmployerAccountId, UpdatedByName, OriginalState, UpdatedState)
VALUES
	(@commitmentId, @apprenticeshipId, @userId, @updatedByRole, @changeType, @createdOn, @providerId, @employerAccountId, @updatedByName, @originalState, @updatedState)