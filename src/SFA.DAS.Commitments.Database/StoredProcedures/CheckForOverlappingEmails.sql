CREATE PROCEDURE [dbo].[CheckForOverlappingEmails]
	@Email NVARCHAR(200),
	@StartDate DATETIME2,
	@EndDate DATETIME2,
	@ApprenticeshipId INT,
	@CohortId INT = NULL
AS
BEGIN
	DECLARE @emails dbo.[EmailCheckTable] 

	INSERT INTO @emails (RowId, Email, StartDate, EndDate, ApprenticeshipId)
	SELECT 1, @Email, @StartDate, @EndDate, @ApprenticeshipId

	EXEC CheckForOverlappingEmailsForTable @emails, @CohortId
END