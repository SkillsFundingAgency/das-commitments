CREATE PROCEDURE [dbo].[GetDataLockStatusesByApprenticeshipId]
	@ApprenticeshipId BIGINT
AS

	SELECT *
	FROM 
		DataLockStatus
	WHERE 
		ApprenticeshipId = @ApprenticeshipId
	ORDER BY
		IlrEffectiveFromDate, Id
