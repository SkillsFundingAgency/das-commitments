CREATE PROCEDURE [dbo].[GetDataLockStatusExpiryCandidates]
	@BeforeDate DATETIME 	
AS

	SELECT 
		*
	FROM 
		[dbo].[DataLockStatus]
	WHERE 
		[IlrEffectiveFromDate] < @BeforeDate
	AND
		[IsExpired] = 0
