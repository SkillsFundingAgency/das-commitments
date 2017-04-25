CREATE PROCEDURE [dbo].[GetLastDataLockEventId]
AS
	SELECT MAX(DataLockEventId) 'DataLockEventId' from DataLockStatus
