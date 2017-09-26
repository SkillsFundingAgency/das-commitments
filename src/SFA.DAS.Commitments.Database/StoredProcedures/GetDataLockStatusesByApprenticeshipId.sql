CREATE PROCEDURE [dbo].[GetDataLockStatusesByApprenticeshipId]
	@ApprenticeshipId BIGINT
AS

	SELECT * FROM DataLockStatus
	WHERE ApprenticeshipId = @ApprenticeshipId
	AND EventStatus <> 3
	AND IsExpired = 0
	ORDER BY
		IlrEffectiveFromDate, Id
