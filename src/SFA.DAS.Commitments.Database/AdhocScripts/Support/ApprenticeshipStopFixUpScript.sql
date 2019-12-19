/*
Apprenticeship Stop fix-up script
This script stops a specific apprenticeship as at a given date. If the apprenticeship is already stopped, the stop date will be amended accordingly.
IMPORTANT: Other services such as Payments that require notification of stops must be informed, or the data they hold will be out of sync.
*/
  
--Set the target record here:
declare @ApprenticeshipId bigint = 0 -- This is the specific apprenticeship record you want to stop or correct
declare @Uln varchar(max) = 'xxxxxxxxxx' -- For extra protection against errors, also specify the ULN for the apprenticeship
declare @StopDate DATETIME = 'yyyy-mm-dd' /* YYYY-MM-DD */ --This is the date you want to stop the apprenticeship (or correct to)

/* ========================================================================== */
/* =================== DO NOT MODIFY BELOW THIS LINE ======================== */
/* ========================================================================== */
   
BEGIN TRAN
   
    --Just some vars here
    DECLARE @error INT
	declare @originalApprenticeId BIGINT
	declare @originalUln VARCHAR(10)
    declare @originalPaymentStatus BIGINT
    declare @originalStartDate DATETIME
    declare @originalStopDate DATETIME
    declare @originalHistoryId BIGINT
	declare @originalHistoryJson NVARCHAR(MAX)
	declare @historyJson NVARCHAR(MAX)
   
    /* Read some data */
   
    select @originalApprenticeId = Id, @originalUln = Uln, @originalPaymentStatus = PaymentStatus, @originalStopDate = StopDate, @originalStartDate = StartDate
	from Apprenticeship where Id = @ApprenticeshipId 
           
    select top 1 @originalHistoryId = Id, @originalHistoryJson = UpdatedState from History where ApprenticeshipId = @originalApprenticeId order by Id desc
  
    /* End data read */
   
    /* Validation checks - */
    IF(@originalApprenticeId is null) BEGIN SET @error=50001 PRINT 'ERROR - Apprenticeship record not found' GOTO batch_abort END
	IF(@originalUln <> @Uln) BEGIN SET @error=50002 PRINT 'ERROR - Apprenticeship ULN does not match' GOTO batch_abort END
	IF(@originalPaymentStatus = 0) BEGIN SET @error=50003 PRINT 'ERROR - Apprenticeship is not approved so cannot be stopped' GOTO batch_abort END
	IF(@StopDate < @originalStartDate) BEGIN SET @error=50004 PRINT 'ERROR - Stop date cannot be before start date' GOTO batch_abort END 
    /* End Validation */
   
    print 'Original Apprenticeship Id: ' + convert(varchar, @originalApprenticeId)
    print 'Original Start Date:' + convert(varchar, @originalStartDate, 126)
    print 'Original Stop Date:' + convert(varchar, @originalStopDate, 126)
   
    /* Backdate the original apprenticeship Stop Date */
    update Apprenticeship set PaymentStatus=3, StopDate = @StopDate where Id = @originalApprenticeId
       
    if(@@ERROR != 0) BEGIN SET @error = @@ERROR GOTO batch_abort END
    print 'New apprenticeship stop date:' + convert(varchar, @StopDate, 126)
    print ''
   
    /* History */

	if(@originalHistoryId is null) begin
		print 'No History record found - history will not be written'
	end else begin

		set @historyJson = JSON_MODIFY(@originalHistoryJson,'$.PaymentStatus', 3)	
		set @historyJson = JSON_MODIFY(@historyJson,'$.StopDate',CONVERT(varchar(50),@stopDate,126))

		insert into History (ApprenticeshipId, UserId, UpdatedByRole, ChangeType, CreatedOn, ProviderId, EmployerAccountId, UpdatedByName, OriginalState, UpdatedState)
		select
		ApprenticeshipId, 'DataFix', 'Employer', 'Updated', GETDATE(), ProviderId, EmployerAccountId, 'DataFix', @originalHistoryJson, @historyJson
		from History
		where Id = @originalHistoryId

	end   
   
    /* End History */
   
    batch_abort:
           
        IF @error != 0
        BEGIN
            ROLLBACK;
            print 'Rollback performed'
            RAISERROR ('Error(s) occurred', 11, 1);
        END
		ELSE
		BEGIN

			print 'Committing transaction'
			COMMIT
			print 'Completed'

		END
