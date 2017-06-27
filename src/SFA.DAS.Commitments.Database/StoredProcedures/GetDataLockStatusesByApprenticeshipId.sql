CREATE PROCEDURE [dbo].[GetDataLockStatusesByApprenticeshipId]
	@ApprenticeshipId BIGINT
AS

	SELECT *
	FROM DataLockStatus
	WHERE ApprenticeshipId = @ApprenticeshipId
		AND 
		SUBSTRING(PriceEpisodeIdentifier,LEN(PriceEpisodeIdentifier)-9,10) <> '01/08/2017' -- TODO: Remove for datalock v2
	ORDER BY
		IlrEffectiveFromDate, Id


