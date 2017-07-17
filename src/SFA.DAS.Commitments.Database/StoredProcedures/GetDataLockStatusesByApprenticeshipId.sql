CREATE PROCEDURE [dbo].[GetDataLockStatusesByApprenticeshipId]
	@ApprenticeshipId BIGINT
AS

	SELECT *
	FROM 
		DataLockStatus
	WHERE 
		ApprenticeshipId = @ApprenticeshipId
    AND
		SUBSTRING(PriceEpisodeIdentifier,LEN(PriceEpisodeIdentifier)-9,10) <> '01/08/2017' -- TODO: 17/07/2017 - Temporary Fix to new academic year data lock errors.
	ORDER BY
		IlrEffectiveFromDate, Id
