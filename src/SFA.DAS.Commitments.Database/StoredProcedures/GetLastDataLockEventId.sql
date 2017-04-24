CREATE PROCEDURE [dbo].[GetLastDataLockEventId]
AS
	SELECT MAX(DataLockEventId) from DataLockStatus
