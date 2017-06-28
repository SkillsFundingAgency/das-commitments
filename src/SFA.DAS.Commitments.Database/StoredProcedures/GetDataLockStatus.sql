CREATE PROCEDURE [dbo].[GetDataLockStatus]
	@DataLockEventId BIGINT
AS
BEGIN
	
	SELECT *
	FROM 
		DataLockStatus
	WHERE 
		DataLockEventId = @DataLockEventId

END
