CREATE PROCEDURE [dbo].[UpdateDatalockStatusIsExpired]
	@ApprenticeshipId bigint ,
	@PriceEpisodeIdentifier NVARCHAR(255),
	@ExpiredDateTime DATETIME
AS
	UPDATE [dbo].[DataLockStatus]
	SET
		[IsExpired]  = 1,
		[Expired] = @ExpiredDateTime
	WHERE 
		[ApprenticeshipId]  = @ApprenticeshipId
	AND
		[PriceEpisodeIdentifier] = @PriceEpisodeIdentifier	

RETURN 0
