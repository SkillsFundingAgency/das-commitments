CREATE PROCEDURE [dbo].[CheckForOverlappingEmails]
	@Email NVARCHAR(200),
	@StartDate DATETIME2,
	@EndDate DATETIME2,
	@ApprenticeshipId INT,
	@CommitmentId INT = NULL
AS

SELECT 
	A.Id, 
	A.FirstName,
	A.LastName,
	A.DateOfBirth,
	A.CommitmentId,
	A.StartDate,
	A.EndDate,
	A.IsApproved,
	A.Email,
	dbo.CourseDatesOverlap(A.StartDate, A.EndDate, @StartDate, @EndDate) AS OverlapStatus
FROM Apprenticeship A
WHERE 
	CASE 
		WHEN @CommitmentId IS NULL AND A.IsApproved = 1 THEN 1
		WHEN @CommitmentId IS NOT NULL AND A.CommitmentId = @CommitmentId AND A.IsApproved = 0 AND A.StartDate IS NOT NULL AND A.EndDate IS NOT NULL THEN 1
		WHEN A.IsApproved = 1 THEN 1
		ELSE 0
	END = 1
	AND A.Id != ISNULL(@ApprenticeshipId,0) 
	AND A.Email = @Email 
	AND dbo.CourseDatesOverlap(A.StartDate, A.EndDate, @StartDate, @EndDate) >= 1 
