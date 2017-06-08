CREATE PROCEDURE [dbo].[UpdateDataLockTriageStatus]
	@DataLockEventId BIGINT,
	@TriageStatus TINYINT
AS

	update DataLockStatus
	set TriageStatus = @TriageStatus
	where DataLockEventId = @DataLockEventId
