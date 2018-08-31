/*
Data script for DPP-1539: Consume & associate the Agreement ID against existing Commitments, Re-emit latest events part 2
Author: Phil Davies (based on existing script by Chris Foster)
Date: 24/07/2018
Target: Events DB
Description: Re-emits latest apprenticeship event for each apprenticeship with AccountLegalEntityPublicHashedId column populated
    ___    ___     ___    _____    ___              ___    _   _   _____     ___   _   _   _____             ___      ___             ___   __   __   ___    _  _    _____    ___               ___    ___     ___    _____              _               ___     ___     ___     ___      ___   _____             ___     ___     _       ___  __      __  _       _       _    
   | _ \  /   \   / __|  |_   _|  | __|     o O O  / _ \  | | | | |_   _|   | _ \ | | | | |_   _|    o O O  / _ \    | __|    o O O  | __|  \ \ / /  | __|  | \| |  |_   _|  / __|     o O O   | _ \  /   \   | _ \  |_   _|    o O O   / |      o O O  / __|   / __|   | _ \   |_ _|    | _ \ |_   _|    o O O  | _ )   | __|   | |     / _ \ \ \    / / | |     | |     | |   
   |  _/  | - |   \__ \    | |    | _|     o      | (_) | | |_| |   | |     |  _/ | |_| |   | |     o      | (_) |   | _|    o       | _|    \ V /   | _|   | .` |    | |    \__ \    o        |  _/  | - |   |   /    | |     o        | |     o       \__ \  | (__    |   /    | |     |  _/   | |     o       | _ \   | _|    | |__  | (_) | \ \/\/ /  |_|     |_|     |_|   
  _|_|_   |_|_|   |___/   _|_|_   |___|   TS__[O]  \___/   \___/   _|_|_   _|_|_   \___/   _|_|_   TS__[O]  \___/   _|_|_   TS__[O]  |___|   _\_/_   |___|  |_|\_|   _|_|_   |___/   TS__[O]  _|_|_   |_|_|   |_|_\   _|_|_   TS__[O]  _|_|_   TS__[O]  |___/   \___|   |_|_\   |___|   _|_|_   _|_|_   TS__[O]  |___/   |___|   |____|  \___/   \_/\_/  _(_)_   _(_)_   _(_)_  
_| """ |_|"""""|_|"""""|_|"""""|_|"""""| {======|_|"""""|_|"""""|_|"""""|_| """ |_|"""""|_|"""""| {======|_|"""""|_| """ | {======|_|"""""|_| """"|_|"""""|_|"""""|_|"""""|_|"""""| {======|_| """ |_|"""""|_|"""""|_|"""""| {======|_|"""""| {======|_|"""""|_|"""""|_|"""""|_|"""""|_| """ |_|"""""| {======|_|"""""|_|"""""|_|"""""|_|"""""|_|"""""|_| """ |_| """ |_| """ | 
"`-0-0-'"`-0-0-'"`-0-0-'"`-0-0-'"`-0-0-'./o--000'"`-0-0-'"`-0-0-'"`-0-0-'"`-0-0-'"`-0-0-'"`-0-0-'./o--000'"`-0-0-'"`-0-0-'./o--000'"`-0-0-'"`-0-0-'"`-0-0-'"`-0-0-'"`-0-0-'"`-0-0-'./o--000'"`-0-0-'"`-0-0-'"`-0-0-'"`-0-0-'./o--000'"`-0-0-'./o--000'"`-0-0-'"`-0-0-'"`-0-0-'"`-0-0-'"`-0-0-'"`-0-0-'./o--000'"`-0-0-'"`-0-0-'"`-0-0-'"`-0-0-'"`-0-0-'"`-0-0-'"`-0-0-'"`-0-0-' 
                                                                                                                                                                                                                                                                                                                                                                                                                                   
*/

declare @sourcedata as table (EmployerAccountId bigint, LegalEntityId nvarchar(50), AccountLegalEntityPublicHashedId char(6));
declare @countsourcedata as int
declare @apprenticeshipIds as table (ApprenticeshipId bigint);
declare @currentsourcedatarownumber as int

print convert(varchar,SYSDATETIME()) + ' Inserting sourcedata'

/* vvv INSERT OUTPUT FROM COMMITMENTS PART OF SCRIPT HERE vvv */

---> overwrite me please <---

/* ^^^ END INSERT OUTPUT HERE ^^^ */

select @countsourcedata = count(1) from @sourcedata

print convert(varchar,SYSDATETIME()) + ' Inserted ' + convert(varchar, @countsourcedata) + ' rows into sourcedata'

--DECLARE @debugview_sourcedata XML = (SELECT * FROM @sourcedata FOR XML AUTO)

SET NOCOUNT ON

declare @EmployerAccountId BIGINT
declare @LegalEntityId nvarchar(50)
declare @AccountLegalEntityPublicHashedId char(6)
declare @ApprenticeshipId BIGINT
declare @Rows int
declare @OriginalEventId BIGINT
declare @OriginalEventAccountLegalEntityPublicHashedId char(6)
declare @NewEventId BIGINT

DECLARE Source_Cursor CURSOR FAST_FORWARD FOR
	select EmployerAccountId, LegalEntityId, AccountLegalEntityPublicHashedId from @sourcedata
	
OPEN Source_Cursor;

FETCH NEXT FROM Source_Cursor INTO @EmployerAccountId, @LegalEntityId, @AccountLegalEntityPublicHashedId
	
set @currentsourcedatarownumber = 1

WHILE @@FETCH_STATUS = 0  
BEGIN

	print convert(varchar,SYSDATETIME()) + ' Source Row #' + convert(varchar,@currentsourcedatarownumber) + ' Fetching all apprenticeship id(s) for EmployerAccountId ' + convert(varchar, @EmployerAccountId) + ', LegalEntityId ' + convert(varchar, @LegalEntityId) + ' (AccountLegalEntityPublicHashedId: ' + @AccountLegalEntityPublicHashedId + ')'
	set @currentsourcedatarownumber = @currentsourcedatarownumber + 1

	delete @apprenticeshipIds

	insert @apprenticeshipIds
	select distinct ApprenticeshipId from ApprenticeshipEvents
	where EmployerAccountId = @EmployerAccountId
	and LegalEntityId = @LegalEntityId

	SELECT @Rows=@@ROWCOUNT

	if @Rows = 0
	begin
		print convert(varchar,SYSDATETIME()) + ' No apprenticeships found'
	end
	else
	begin

		--DECLARE @debugview_apprenticeshipids XML = (SELECT * FROM @apprenticeshipIds FOR XML AUTO)

		print convert(varchar,SYSDATETIME()) + ' Found ' + convert(varchar,@Rows) + ' Apprenticeships'

		DECLARE ApprenticeshipsId_Cursor CURSOR FAST_FORWARD FOR
			select ApprenticeshipId from @apprenticeshipIds;
		
		OPEN ApprenticeshipsId_Cursor;
	
		FETCH NEXT FROM ApprenticeshipsId_Cursor INTO @ApprenticeshipId

		WHILE @@FETCH_STATUS = 0  
		BEGIN
	
			print convert(varchar,SYSDATETIME()) + ' Checking latest event for ApprenticeshipId ' + convert(varchar, @ApprenticeshipId)

			--Re-emit event:

			-- we only re-emit the last event for each apprenticeship id (which is enough for the payments commitment reference data job)
			SELECT
				@OriginalEventId = ae.Id,
				@OriginalEventAccountLegalEntityPublicHashedId = ae.AccountLegalEntityPublicHashedId
			FROM ApprenticeshipEvents ae
			   JOIN (SELECT MAX(Id) as Id FROM ApprenticeshipEvents where apprenticeshipid = @ApprenticeshipId) as max
				  ON ae.Id = max.Id

			if @OriginalEventAccountLegalEntityPublicHashedId is not null
			BEGIN
				print convert(varchar,SYSDATETIME()) + ' No need to re-emit latest event ' + convert(varchar,@OriginalEventId) + ' as it already has AccountLegalEntityPublicHashedId: ' + @OriginalEventAccountLegalEntityPublicHashedId
			END
			ELSE
			BEGIN
				print convert(varchar,SYSDATETIME()) + ' Re-emitting event ' + convert(varchar,@OriginalEventId) + ' for ApprenticeshipId ' + convert(varchar, @ApprenticeshipId) + ' - AccountLegalEntityPublicHashedId: ' + @AccountLegalEntityPublicHashedId

				-- Insert the duplicate event
				INSERT INTO ApprenticeshipEvents
				(   [Event]
					,[CreatedOn]
					,[ApprenticeshipId]
					,[PaymentOrder]
					,[PaymentStatus]
					,[AgreementStatus]
					,[ProviderId]
					,[LearnerId]
					,[EmployerAccountId]
					,[TrainingType]
					,[TrainingId]
					,[TrainingStartDate]
					,[TrainingEndDate]
					,[TrainingTotalCost]
					,[LegalEntityId]
					,[LegalEntityName]
					,[LegalEntityOrganisationType]
					,[EffectiveFrom]
					,[EffectiveTo]
					,[DateOfBirth]
					,[TransferSenderId]
					,[TransferSenderName]
					,[TransferApprovalStatus]
					,[TransferApprovalActionedOn]
					,[AccountLegalEntityPublicHashedId]
				)
				SELECT 
					[Event]
					,GETDATE()
					,[ApprenticeshipId]
					,[PaymentOrder]
					,[PaymentStatus]
					,[AgreementStatus]
					,[ProviderId]
					,[LearnerId]
					,[EmployerAccountId]
					,[TrainingType]
					,[TrainingId]
					,[TrainingStartDate]
					,[TrainingEndDate]
					,[TrainingTotalCost]
					,[LegalEntityId]
					,[LegalEntityName]
					,[LegalEntityOrganisationType]
					,[EffectiveFrom]
					,[EffectiveTo]
					,[DateOfBirth]
					,[TransferSenderId]
					,[TransferSenderName]
					,[TransferApprovalStatus]
					,[TransferApprovalActionedOn]
					,@AccountLegalEntityPublicHashedId
				FROM ApprenticeshipEvents
				WHERE Id = @OriginalEventId
				--ORDER BY Id asc

				-- Get the id of the event insert
				SELECT @NewEventId = SCOPE_IDENTITY()

				-- Add for the new event
				INSERT INTO PriceHistory
				(
					[ApprenticeshipEventsId]
					,[TotalCost]
					,[EffectiveFrom]
					,[EffectiveTo]
				)
				SELECT
				@NewEventId,
				h.TotalCost,
				h.EffectiveFrom,
				h.EffectiveTo
				FROM PriceHistory h
				where h.ApprenticeshipEventsId = @OriginalEventId
			END

			FETCH NEXT FROM ApprenticeshipsId_Cursor INTO @ApprenticeshipId
		
		--End Re-emit event
		END;

		CLOSE ApprenticeshipsId_Cursor;
		DEALLOCATE ApprenticeshipsId_Cursor;
	end

FETCH NEXT FROM Source_Cursor INTO @EmployerAccountId, @LegalEntityId, @AccountLegalEntityPublicHashedId
END;

CLOSE Source_Cursor
DEALLOCATE Source_Cursor

print convert(varchar,SYSDATETIME()) + ' That''s all folks!'
