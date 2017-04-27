CREATE PROCEDURE [dbo].[GetDataLockStatus]
	@DataLockEventId BIGINT
AS
BEGIN
	
	SELECT *
	from DataLockStatus
	where DataLockEventId = @DataLockEventId

END
