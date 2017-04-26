CREATE PROCEDURE [dbo].[GetDataLockStatusesByApprenticeshipId]
	@ApprenticeshipId BIGINT
AS

	SELECT *
	from DataLockStatus
	where ApprenticeshipId = @ApprenticeshipId


