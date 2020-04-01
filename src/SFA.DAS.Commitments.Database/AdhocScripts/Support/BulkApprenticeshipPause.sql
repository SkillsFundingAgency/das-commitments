/*
Bulk Apprenticeship Pause script (by Employer Account Id)
This script pauses all apprenticeships for specified employers as at a given date.
If an apprenticeship is already paused, stopped, or completed, it will be ignored and no change will be made

CAUTION: - Data modification scripts are dangerous: there may be no way to undo mistakes.
         - Some validation is included, but it may be possible to put the system into an invalid state.
		 - Services external to the Approvals product may be left in an inconsistent state.
*/

set nocount on;

declare @employers TABLE (
	accountId bigint not null,
	pauseDate date not null
);
declare @apprenticeshipId bigint
declare @pauseDate DATETIME

insert into @employers
values 
/* ========================================================================== */
/* =================== DO NOT MODIFY ABOVE THIS LINE ======================== */
/* ========================================================================== */
-- put employer account Ids (numeric) and pause dates (iso-format) here e.g.
(30060,'2020-01-01'),
(8194,'2020-03-01')
;
/* ========================================================================== */
/* =================== DO NOT MODIFY BELOW THIS LINE ======================== */
/* ========================================================================== */

declare cur cursor local for
with apprenticesToPause (apprenticeshipId, pauseDate) as (
	select a.Id, e.pauseDate
	from Apprenticeship a
	join Commitment c
		on c.Id = a.CommitmentId
	join @employers e
		on e.accountId = c.EmployerAccountId
	where PaymentStatus = 1 -- active
)
select * from apprenticesToPause
open cur 
fetch next from cur into @apprenticeshipId, @pauseDate

while @@FETCH_STATUS = 0 begin
	BEGIN TRAN
	  
    --Just some vars here
    DECLARE @error INT
    declare @originalHistoryId BIGINT
	declare @originalHistoryJson NVARCHAR(MAX)
	declare @historyJson NVARCHAR(MAX)
   
    /* Read some data */           
    select top 1 @originalHistoryId = Id, @originalHistoryJson = UpdatedState from History where ApprenticeshipId = @apprenticeshipId order by Id desc
    /* End data read */
   
    print '-- Apprenticeship Id: ' + convert(varchar, @apprenticeshipId)
    
    /* Pause the record */
    update Apprenticeship set PaymentStatus=2, PauseDate = @pauseDate where Id = @apprenticeshipId
       
    if(@@ERROR != 0) BEGIN SET @error = @@ERROR GOTO batch_abort END
    print '-- Apprenticeship pause date: ' + convert(varchar, @pauseDate, 126)
   
    /* History */

	if(@originalHistoryId is null) begin
		print '-- No History record found - history will not be written'
	end else begin

		set @historyJson = JSON_MODIFY(@originalHistoryJson,'$.PaymentStatus', 2)	
		set @historyJson = JSON_MODIFY(@historyJson,'$.PauseDate',CONVERT(varchar(50),@pauseDate,126))

		insert into History (EntityType, ApprenticeshipId, UserId, UpdatedByRole, ChangeType, CreatedOn, ProviderId, EmployerAccountId, UpdatedByName, OriginalState, UpdatedState)
		select
		'Apprenticeship', ApprenticeshipId, 'DataFix', 'Employer', 'Paused', GETDATE(), ProviderId, EmployerAccountId, 'DataFix', @originalHistoryJson, @historyJson
		from History
		where Id = @originalHistoryId

	end   
   
    /* End History */

   
batch_abort:
           
    IF @error != 0
    BEGIN
        ROLLBACK;
        print '-- Rollback performed'
        RAISERROR ('-- Error(s) occurred', 11, 1);
    END
	ELSE
	BEGIN

		print '-- Committing transaction for ApprenticeshipId [' + cast(@apprenticeshipId as varchar) + ']'
		COMMIT -- use ROLLBACK for dev'ing
		print '-- Completed'

	END
	print '-- ================================================================================'
	fetch next from cur into @apprenticeshipId, @pauseDate
end

close cur
deallocate cur

set nocount off;