/*
Bulk Apprenticeship Resume script (by Employer Account Id)
This script pauses all apprenticeships for specified employers with immediate effect.
If an apprenticeship is already paused, stopped, or completed, it will be ignored and no change will be made.

CAUTION: - Data modification scripts are dangerous: there may be no way to undo mistakes.
         - Some validation is included, but it may be possible to put the system into an invalid state.
		 - Services external to the Approvals product may be left in an inconsistent state.
*/

set nocount on;

declare @employers TABLE (
	accountId bigint not null
);
declare @apprenticeshipId bigint

insert into @employers
values 
/* ========================================================================== */
/* =================== DO NOT MODIFY ABOVE THIS LINE ======================== */
/* ========================================================================== */
-- put employer account Ids (numeric) here
(30060)
;
/* ========================================================================== */
/* =================== DO NOT MODIFY BELOW THIS LINE ======================== */
/* ========================================================================== */

declare cur cursor local for
with apprenticesToResume (apprenticeshipId) as (
	select a.Id
	from Apprenticeship a
	join Commitment c
		on c.Id = a.CommitmentId
	join @employers e
		on e.accountId = c.EmployerAccountId
	where PaymentStatus = 2 -- paused
)
select * from apprenticesToResume
open cur 
fetch next from cur into @apprenticeshipId

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
    
    /* Resume the record */
    update Apprenticeship set PaymentStatus=1, PauseDate = null where Id = @apprenticeshipId
       
    if(@@ERROR != 0) BEGIN SET @error = @@ERROR GOTO batch_abort END
   
    /* History */

	if(@originalHistoryId is null) begin
		print '-- No History record found - history will not be written'
	end else begin

		set @historyJson = JSON_MODIFY(@originalHistoryJson,'$.PaymentStatus', 1)	
		set @historyJson = JSON_MODIFY(@historyJson,'strict $.PauseDate',null)

		insert into History (ApprenticeshipId, UserId, UpdatedByRole, ChangeType, CreatedOn, ProviderId, EmployerAccountId, UpdatedByName, OriginalState, UpdatedState)
		select
		ApprenticeshipId, 'DataFix', 'Employer', 'Resumed', GETDATE(), ProviderId, EmployerAccountId, 'DataFix', @originalHistoryJson, @historyJson
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
	fetch next from cur into @apprenticeshipId
end

close cur
deallocate cur

set nocount off;