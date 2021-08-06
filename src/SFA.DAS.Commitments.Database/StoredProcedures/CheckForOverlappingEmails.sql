CREATE PROCEDURE [dbo].[CheckForOverlappingEmails]
	@Email NVARCHAR(200),
	@StartDate DATETIME2,
	@EndDate DATETIME2,
	@ApprenticeshipId INT,
	@CohortId INT = NULL
AS

SELECT 
	A.Id, 
	A.FirstName,
	A.LastName,
	A.DateOfBirth,
	A.CommitmentId AS CohortId,
	A.StartDate,
	A.EndDate,
	A.IsApproved,
	A.Email,
	dbo.CourseDatesOverlap(A.StartDate, dbo.GetEndDateForOverlapChecks(A.PaymentStatus, A.EndDate, A.StopDate, A.CompletionDate), @StartDate, @EndDate) AS OverlapStatus
FROM Apprenticeship A
WHERE 
	CASE 
		WHEN @CohortId IS NULL AND A.IsApproved = 1 THEN 1
		WHEN @CohortId IS NOT NULL AND A.CommitmentId = @CohortId AND A.IsApproved = 0 AND A.StartDate IS NOT NULL AND A.EndDate IS NOT NULL THEN 1
		WHEN A.IsApproved = 1 THEN 1
		ELSE 0
	END = 1
	AND A.Id != ISNULL(@ApprenticeshipId,0) 
	AND A.Email = @Email 
	AND dbo.CourseDatesOverlap(A.StartDate, dbo.GetEndDateForOverlapChecks(A.PaymentStatus, A.EndDate, A.StopDate, A.CompletionDate), @StartDate, @EndDate) >= 1 
