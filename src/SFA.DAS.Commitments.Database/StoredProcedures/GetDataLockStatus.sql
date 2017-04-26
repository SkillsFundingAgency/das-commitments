CREATE PROCEDURE [dbo].[GetDataLockStatus]
	@DataLockId BIGINT
AS
BEGIN
	
	SELECT *
	from DataLockStatus
	where Id = @DataLockId

END
