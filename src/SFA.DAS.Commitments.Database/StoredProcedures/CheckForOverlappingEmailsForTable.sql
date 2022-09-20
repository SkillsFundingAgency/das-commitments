CREATE PROCEDURE [dbo].[CheckForOverlappingEmailsForTable]
	@Emails [EmailCheckTable] READONLY,
	@CohortId BIGINT = NULL
AS
BEGIN
	SELECT 
		E.RowId,
		A.Id, 
		A.FirstName,
		A.LastName,
		A.DateOfBirth,
		A.CommitmentId AS CohortId,
		A.StartDate,
		A.EndDate,
		A.IsApproved,
		A.Email,
		dbo.CourseDatesOverlap(A.StartDate, dbo.GetEndDateForOverlapChecks(A.PaymentStatus, A.EndDate, A.StopDate, A.CompletionDate), E.StartDate, E.EndDate) AS OverlapStatus
	FROM Apprenticeship A
	JOIN @Emails E ON E.Email = A.Email
	JOIN Commitment C ON A.CommitmentId = C.Id
	WHERE 
		CASE 
			WHEN @CohortId IS NOT NULL AND A.CommitmentId = @CohortId AND A.IsApproved = 0 AND A.StartDate IS NOT NULL AND A.EndDate IS NOT NULL THEN 1
			WHEN C.WithParty = 4 AND A.IsApproved = 0 THEN 1
			WHEN A.IsApproved = 1 THEN 1
			ELSE 0
		END = 1
		AND NOT (A.StopDate IS NOT NULL AND A.StartDate = A.StopDate)
		AND A.Id != ISNULL(E.ApprenticeshipId,0) 
		AND A.Email = E.Email 
		AND dbo.CourseDatesOverlap(A.StartDate, dbo.GetEndDateForOverlapChecks(A.PaymentStatus, A.EndDate, A.StopDate, A.CompletionDate), E.StartDate, E.EndDate) >= 1 
END