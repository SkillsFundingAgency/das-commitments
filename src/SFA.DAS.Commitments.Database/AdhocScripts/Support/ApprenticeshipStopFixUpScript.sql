/*
Apprenticeship Stop fix-up script
This script stops a specific apprenticeship as at a given date. If the apprenticeship is already stopped, the stop date will be amended accordingly.
IMPORTANT: Also generates a SQL statement to be run against the Events DB to keep things in sync.
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
    declare @historyJson VARCHAR(MAX)
   
    /* Read some data */
   
    select @originalApprenticeId = Id, @originalUln = Uln, @originalPaymentStatus = PaymentStatus, @originalStopDate = StopDate, @originalStartDate = StartDate
	from Apprenticeship where Id = @ApprenticeshipId 
   
    --select @originalChangeStateHistoryId = Id from History where EntityType = 'Apprenticeship' and EntityId = @originalApprenticeId and ChangeType = 'ChangeOfStatus'
       
    --select @historyJson = UpdatedState from History where Id = @originalChangeStateHistoryId
   
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
    print 'Back-dated apprenticeship stop date:' + convert(varchar, @StopDate, 126)
    print ''
   
    /* History */
   
    --History does not need to be written as StopDate is not written to history anyway!
    --set @historyJson = JSON_MODIFY(@historyJson,'$.StopDate',CONVERT(varchar(50),@originalStartDate,126))
   
    /* End History */
   
    /* Generate statement to run against Events DB */
    declare @eventsInsertSql VARCHAR(MAX);
   
    WITH TargetApprenticeship (
     ApprenticeshipId, ULN, ProviderId, EmployerAccountId, LegalEntityId, LegalEntityName, LegalEntityOrganisationType,
     NewPaymentOrder, PaymentStatus, AgreementStatus, TrainingType, TrainingCode, StartDate, EndDate, Cost, DateOfBirth,
	 TransferSenderId, TransferSenderName, TransferApprovalStatus, TransferApprovalActionedOn, StopDate, PauseDate
    )
    AS (
        SELECT
            CAST(a.Id as varchar(20)) AS ApprenticeshipId,
            a.ULN AS ULN,
            CAST(c.ProviderId AS varchar(20)) AS ProviderId,
            CAST(c.EmployerAccountId AS varchar(20)) AS EmployerAccountId,
            CAST(c.LegalEntityId AS varchar(40)) AS LegalEntityId,
            REPLACE(c.LegalEntityName,'''', '''''' ) AS LegalEntityName,
            CASE LegalEntityOrganisationType
                WHEN 1 THEN 'CompaniesHouse'
                WHEN 2 THEN 'Charities'
                WHEN 3 THEN 'PublicBodies'
                WHEN 4 THEN 'Other'
            END AS LegalEntityOrganisationType,
            CAST(a.PaymentOrder AS varchar(10)) AS PaymentOrder,
            CAST(PaymentStatus AS varchar(10)) AS PaymentStatus,
            CAST(AgreementStatus AS varchar(10)) AS AgreementStatus,
            CASE TrainingType WHEN 1 THEN '0' ELSE '1' END,
            CAST(TrainingCode AS varchar(10)) AS TrainingCode,
            CONVERT(varchar(50),StartDate,126) AS StartDate,
            CONVERT(varchar(50),EndDate,126) AS EndDate,
            CAST(Cost AS varchar(20)) AS Cost,
            CONVERT(varchar(50),DateOfBirth,126) AS DateOfBirth,
			CAST(TransferSenderId as varchar(20)) as TransferSenderId,
			TransferSenderName,
			CAST(TransferApprovalStatus as varchar(10)) as TransferApprovalStatus,
			CONVERT(varchar(50),TransferApprovalActionedOn,126) as TransferApprovalActionedOn,
			CONVERT(varchar(50),StopDate,126) as StopDate,
			CONVERT(varchar(50),PauseDate,126) as PauseDate
        FROM Commitment c
        INNER JOIN Apprenticeship a
        on c.Id = a.CommitmentId
        WHERE a.Id = @originalApprenticeId
    )
    SELECT @eventsInsertSql =
    'INSERT INTO [dbo].[ApprenticeshipEvents] ([Event],[CreatedOn],[ApprenticeshipId],[PaymentOrder],[PaymentStatus],[AgreementStatus],[ProviderId],[LearnerId],[EmployerAccountId],[TrainingType],[TrainingId],[TrainingStartDate],[TrainingEndDate],[TrainingTotalCost],[LegalEntityId],[LegalEntityName],[LegalEntityOrganisationType],[EffectiveFrom],[EffectiveTo],[DateOfBirth],TransferSenderId, TransferSenderName, TransferApprovalStatus,TransferApprovalActionedOn,StoppedOnDate,PausedOnDate)
    VALUES (''APPRENTICESHIP-UPDATED'',''' + convert(varchar(50),getdate(),126) + ''','
    + ApprenticeshipId + ','
    + NewPaymentOrder + ','
    + PaymentStatus + ','
    + AgreementStatus + ','
    + ProviderId + ','
    + ULN + ','
    + EmployerAccountId + ','
    + TrainingType + ','''
    + TrainingCode + ''','''
    + StartDate + ''','''
    + EndDate + ''','
    + Cost +','''
    + LegalEntityId + ''','''
    + LegalEntityName + ''','''
    + LegalEntityOrganisationType + ''','''
    + convert(varchar, @StopDate, 126)
    + ''',NULL,'''
    + DateOfBirth + ''','
	+ CASE WHEN TransferSenderId is null then 'NULL' else '''' + TransferSenderId + '''' END + ','
	+ '''' + TransferSenderName + ''','
	+ CASE WHEN TransferApprovalStatus is null then 'NULL' else '''' + TransferApprovalStatus + '''' END + ','
	+ CASE WHEN TransferApprovalActionedOn is null then 'NULL' else '''' + TransferApprovalActionedOn + '''' END + ','
	+ CASE WHEN StopDate is null then 'NULL' else '''' + StopDate + '''' END + ','
	+ CASE WHEN PauseDate is null then 'NULL' else '''' + PauseDate + '''' END
	+ ')'
    FROM TargetApprenticeship
  
  
    declare @eventsPriceHistoryInsertSql nvarchar(max)
      
  
    set @eventsPriceHistoryInsertSql = 'declare @eventId bigint = SCOPE_IDENTITY();' + CHAR(13) + CHAR(13)
      
  
    select
    @eventsPriceHistoryInsertSql +=
        'INSERT INTO PriceHistory(ApprenticeshipEventsId, TotalCost, EffectiveFrom, EffectiveTo)' + CHAR(13)
        + 'VALUES (@eventId,' + convert(varchar,Cost)
        + ',''' + convert(varchar,FromDate,120) + ''','
        + COALESCE('''' + (CONVERT(varchar,ToDate,120) + ''''), 'NULL') + ');'
        + CHAR(13) + CHAR(13)
    from PriceHistory where ApprenticeshipId = @originalApprenticeId
  
  
    print 'Generated Sql statements to be run against Events DB:'
    print ''
    print @eventsInsertSql
    print''
    print @eventsPriceHistoryInsertSql
  
    /* End Events DB insert statement generation */
   
    batch_abort:
           
        IF @error != 0
        BEGIN
            ROLLBACK;
            print 'Rollback performed'
            RAISERROR ('Error(s) occurred', 11, 1);
        END
    
COMMIT
