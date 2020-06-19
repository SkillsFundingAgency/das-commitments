/*
UpdateAccountLevyStatus script for Commitments db
Sets the LevyStatus for a given Account (0 = NonLevy, 1 = Levy)
Note: this script only updates the record held in the Commitments service
Other systems (eas, reservations, payments, etc.) must be updated accordingly
Commitment records that have been created and approved under the incorrect status are not updated.
*/

--Declare the AccountId and the correct LevyStatus here
declare @AccountId BIGINT = 29645
declare @NewLevyStatus TINYINT = 0
---

/* ***************************** */
/* DO NOT MODIFY BELOW THIS LINE */ 
/* ***************************** */

SET NOCOUNT ON

print 'Updating Levy Status for Account ' + convert(varchar, @AccountId) + ' to ' + convert(varchar, @NewLevyStatus)

declare @CurrentLevyStatus TINYINT
select @CurrentLevyStatus = LevyStatus from Accounts where Id = @AccountId

if(@CurrentLevyStatus is null)
BEGIN
	print 'Error - could not find Account ' + convert(varchar, @AccountId)
	return
END

if(@CurrentLevyStatus = @NewLevyStatus)
BEGIN
	print 'Error - Levy Status for Account ' + convert(varchar, @AccountId) + ' is already ' + convert(varchar,@NewLevyStatus)
	return
END

update Accounts set LevyStatus = @NewLevyStatus where Id = @AccountId
print 'Account levy status updated ok'

IF(EXISTS(SELECT * FROM Commitment where EmployerAccountId = @AccountId))
BEGIN

	print ''
	print 'Warning: this Account has Commitments (or drafts) - these have not been updated by this script'

END

SET NOCOUNT OFF