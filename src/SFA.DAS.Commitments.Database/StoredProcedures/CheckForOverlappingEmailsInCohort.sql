CREATE PROCEDURE [dbo].[CheckForOverlappingEmailsInCohort](@CohortId INT)
AS
BEGIN
	DECLARE @emails dbo.[EmailCheckTable] 

	INSERT INTO @emails (RowId, Email, StartDate, EndDate, ApprenticeshipId)
	SELECT Id, Email, StartDate, EndDate, Id FROM Apprenticeship WHERE CommitmentId = @CohortId AND Email IS NOT NULL AND StartDate IS NOT NULL AND EndDate IS NOT NULL

	EXEC CheckForOverlappingEmailsForTable @emails, @CohortId
END