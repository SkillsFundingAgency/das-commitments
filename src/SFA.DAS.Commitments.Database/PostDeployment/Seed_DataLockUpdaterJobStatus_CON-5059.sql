--CON-5059 - Seed DataLockUpdaterJobStatus table with last event id

IF(NOT EXISTS(SELECT * FROM DataLockUpdaterJobStatus))
BEGIN

	INSERT INTO DataLockUpdaterJobStatus(LastEventId)
	SELECT MAX(DataLockEventId) from DataLockStatus

END
