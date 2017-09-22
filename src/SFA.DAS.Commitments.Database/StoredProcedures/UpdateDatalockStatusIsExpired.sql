CREATE PROCEDURE [dbo].[UpdateDatalockStatusIsExpired]
	@ApprenticeshipId bigint ,
	@PriceEpisodeIdentifier NVARCHAR(255)
	WITH RECOMPILE
AS
	UPDATE [dbo].[DataLockStatus]
	SET
		[IsExpired]  = 1,
		[Expired] = GETDATE()
	WHERE 
		[ApprenticeshipId]  = @ApprenticeshipId
	AND
		[PriceEpisodeIdentifier] = @PriceEpisodeIdentifier
	

RETURN 0
