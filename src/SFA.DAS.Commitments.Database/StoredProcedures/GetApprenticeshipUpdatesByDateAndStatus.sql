CREATE PROCEDURE [dbo].[GetApprenticeshipUpdatesByDateAndStatus]
	@date DATETIME,
	@status tinyint
AS
	SELECT au.* 
	FROM Apprenticeship a
	INNER JOIN
	ApprenticeshipUpdate au
	ON au.ApprenticeshipId = a.ID
	WHERE au.Status = @status
	AND a.StartDate < @date

RETURN 0
