CREATE PROCEDURE [dbo].[UpdateDataLockTriageStatus]
	@DataLockEventId BIGINT,
	@TriageStatus TINYINT,
	@ApprenticeshipUpdateId BIGINT
AS

	update DataLockStatus
	set TriageStatus = @TriageStatus,
	ApprenticeshipUpdateId = @ApprenticeshipUpdateId
	where DataLockEventId = @DataLockEventId
