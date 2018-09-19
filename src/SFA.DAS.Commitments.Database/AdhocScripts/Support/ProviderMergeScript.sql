/*
This script merges two Providers. It does so by stopping and backdating any apprenticeships that are active (or paused)
after the end of the academic year and creating an exact clone of the original except that it will be for the new provider.
Apprenticeships that are part of a Transfer are NOT currently covered by this script and will be ignored.

See parameters block below.
*/

SET XACT_ABORT ON;
GO
ALTER TABLE Apprenticeship add CloneOf BIGINT NULL;
GO

/* ############################ Parameters ############################ */

declare @oldProviderId bigint = xxxxx; -- UKPRN of the discontinued provider
declare @newProviderId bigint = xxxxx; -- UKPRN of the continuing provider 
declare @newProviderName nvarchar(100) = null; --The new name of the continuing provider, if applicable, or NULL if no change
declare @academicYearStartDate datetime = '2018-08-01'; --The date of the new academic year - apprenticeships before this date will not be affected

/* 
############################ DON'T GO CHANGING THINGS BELOW THIS LINE! ############################
*/

SET NOCOUNT ON;

BEGIN TRAN

--0. Get provider name
--1. Get the target apprenticeships
--2. Create a single commitment per employer account / legal entity combination
--3. Insert a new apprenticeship for each target, for the new UKPRN
--4. Stop and backdate the target apprenticeships
--5. Emit events for all new and changed apprenticeships

--0. Get providername
select top 1 @newProviderName = COALESCE(@newProviderName, ProviderName) from Commitment where ProviderId = @newProviderId order by id desc

print 'Merging apprenticeships for ' + convert(varchar,@oldProviderId) + ' into provider account ' + convert(varchar,@newProviderId) + ' ' + @newProviderName


--1. Get the target apprenticeships
declare @apprenticeshipTargets as table (ApprenticeshipId bigint)

insert into @apprenticeshipTargets
select Id as ApprenticeshipId
from ApprenticeshipSummary
where ProviderId = @oldProviderId --target the discontinued provider only!
and EndDate >= @academicYearStartDate -- ignore those that ended prior to academic year
and (StopDate is null OR StopDate >= @academicYearStartDate) --ignore those stopped prior to academic year
And (StopDate is null OR StopDate != StartDate) --ignore stopped and backdated
And TransferSenderId is null

--2. Create a single commitment per employer account / legal entity combination

print 'Creating new commitments'

INSERT INTO [dbo].[Commitment]
([Reference]
,[EmployerAccountId]
,[LegalEntityId]
,[LegalEntityName]
,[LegalEntityAddress]
,[LegalEntityOrganisationType]
,[ProviderId]
,[ProviderName]
,[CommitmentStatus]
,[EditStatus]
,[CreatedOn]
,[LastAction]
,[LastUpdatedByEmployerName]
,[LastUpdatedByEmployerEmail]
,[LastUpdatedByProviderName]
,[LastUpdatedByProviderEmail]
--,[TransferSenderId]
--,[TransferSenderName]
--,[TransferApprovalStatus]
--,[TransferApprovalActionedByEmployerName]
--,[TransferApprovalActionedByEmployerEmail]
--,[TransferApprovalActionedOn]
,[AccountLegalEntityPublicHashedId])
select distinct
'' as Reference,
c.EmployerAccountId, c.LegalEntityId, c.LegalEntityName, c.LegalEntityAddress, c.LegalEntityOrganisationType,
@newProviderId as ProviderId, @newProviderName as ProviderName, 1 as CommitmentStatus, 0 as EditStatus, GETDATE() as CreatedOn, 2 as LastAction,
'' as LastUpdatedByEmployerName, '' as LastUpdatedByEmployerEmail, '' as LastUpdatedByProviderName, '' as LastUpdatedByProviderEmail, c.AccountLegalEntityPublicHashedId
from Apprenticeship a
join Commitment c on c.Id = a.CommitmentId
join @apprenticeshipTargets t on a.Id = t.ApprenticeshipId
where c.TransferSenderId is null --CANNOT HANDLE TRANSFERS

print 'Created ' + convert(varchar,@@ROWCOUNT) + ' cohorts'


--3. Insert a new apprenticeship for each target, for the new UKPRN

insert into Apprenticeship
([CommitmentId]
           ,[FirstName]
           ,[LastName]
           ,[ULN]
           ,[TrainingType]
           ,[TrainingCode]
           ,[TrainingName]
           ,[Cost]
           ,[StartDate]
           ,[EndDate]
           ,[AgreementStatus]
           ,[PaymentStatus]
           ,[DateOfBirth]
           ,[NINumber]
           ,[EmployerRef]
           ,[ProviderRef]
           ,[CreatedOn]
           ,[AgreedOn]
           ,[PaymentOrder]
           ,[StopDate]
           ,[PauseDate]
           ,[HasHadDataLockSuccess]
           ,[PendingUpdateOriginator]
           ,[EPAOrgId]
		   ,[CloneOf])
select
(select max(id) from Commitment nc where nc.ProviderId = @newProviderId and nc.EmployerAccountId = c.EmployerAccountId and  nc.LegalEntityId = c.LegalEntityId) CommitmentId,
a.[FirstName]
           ,a.[LastName]
           ,a.[ULN]
           ,a.[TrainingType]
           ,a.[TrainingCode]
           ,a.[TrainingName]
           ,a.[Cost]
           ,a.[StartDate]
           ,a.[EndDate]
           ,a.[AgreementStatus]
           ,a.[PaymentStatus]
           ,a.[DateOfBirth]
           ,a.[NINumber]
           ,a.[EmployerRef]
           ,a.[ProviderRef]
           ,a.[CreatedOn]
           ,a.[AgreedOn]
           ,a.[PaymentOrder]
           ,a.[StopDate]
           ,a.[PauseDate]
           ,a.[HasHadDataLockSuccess]
           ,a.[PendingUpdateOriginator]
           ,a.[EPAOrgId]
		   ,t.ApprenticeshipId as CloneOf
from
@apprenticeshipTargets t
join Apprenticeship a on a.Id = t.ApprenticeshipId
join Commitment c on c.Id = a.CommitmentId --Original commitment

print 'Created ' + convert(varchar,@@ROWCOUNT) + ' apprenticeships'

--Add price history
insert into PriceHistory (ApprenticeshipId, Cost, FromDate, ToDate)
select
na.Id, ph.Cost, ph.FromDate, ph.ToDate
from Apprenticeship na
join Apprenticeship a on na.CloneOf = a.Id
join PriceHistory ph on ph.ApprenticeshipId = a.Id
where a.Id in (select ApprenticeshipId from @apprenticeshipTargets)

print 'Created ' + convert(varchar,@@ROWCOUNT) + ' price history records'

--4. Stop and back-date the target apprenticeships
update Apprenticeship
set StopDate = StartDate, PaymentStatus = 3
where Id in (select ApprenticeshipId from @apprenticeshipTargets)

print 'Stopped and backdated ' + convert(varchar,@@ROWCOUNT) + ' apprenticeships'

--5. Emit an event for the old and new apprenticeship

print ''
print 'Script for execution on Events Db:'
print ''

--Get the old ones
declare @emitEventApprenticeshipId BIGINT
DECLARE apprenticeshipTargetsCursor CURSOR FOR
	select t.ApprenticeshipId from @apprenticeshipTargets t join Apprenticeship a on a.Id = t.ApprenticeshipId
	union all
	select na.Id from Apprenticeship na join Apprenticeship a on na.CloneOf = a.Id where a.Id in (select ApprenticeshipId from @apprenticeshipTargets)

	print 'declare @apprenticeshipEventsId bigint;'

OPEN apprenticeshipTargetsCursor

FETCH NEXT FROM apprenticeshipTargetsCursor INTO @emitEventApprenticeshipId
WHILE @@FETCH_STATUS = 0
BEGIN


	/* BEGIN LIFT 'N' SHIFT EMIT EVENT SCRIPT */
	-- after pasting, move @apprenticeshipEventsId declaration up outside of cursor

		declare @outputScript as nvarchar(max) = ''

		set @outputScript = '';

		select top 1
		@outputScript = 'INSERT INTO [dbo].[ApprenticeshipEvents]
           (
		    [Event]
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
           ,[StoppedOnDate]
           ,[PausedOnDate]
           ,[AccountLegalEntityPublicHashedId]
		   )
		   values
		   (
			''APPRENTICESHIP-UPDATED''
           ,GETDATE()
           ,' + convert(varchar,a.Id) + '
		   ,' + CASE WHEN a.PaymentOrder is null then 'null' else convert(varchar,a.PaymentOrder) end + '
		   ,' + CASE WHEN a.[PaymentStatus] is null then 'null' else  convert(varchar,a.[PaymentStatus]) end + '
		   ,' + CASE WHEN a.[AgreementStatus] is null then 'null' else convert(varchar,a.[AgreementStatus]) end + '		   
           ,' + convert(varchar,c.[ProviderId]) + '
           ,' + CASE WHEN a.[ULN] is null then 'null' else + '''' + convert(varchar,a.[ULN]) + '''' end + '		   
           ,' + convert(varchar,c.[EmployerAccountId]) + '
           ,' + CASE WHEN a.[TrainingType] is null then 'null' WHEN a.TrainingType=0 THEN '1' ELSE '0' END + '		   
		   ,' + CASE WHEN a.[TrainingCode] is null then 'null' else convert(varchar,a.[TrainingCode]) end + '		   
		   ,' + CASE WHEN a.StartDate is null then 'null' else + '''' + convert(varchar(10),a.StartDate,120) + '''' end + '
		   ,' + CASE WHEN a.EndDate is null then 'null' else + '''' + convert(varchar(10),a.EndDate,120) + '''' end + '
		   ,' + CASE WHEN a.Cost is null then 'null' else convert(varchar,a.Cost) end + '		   
		   ,' + '''' + convert(varchar,c.LegalEntityId,120) + '''' + '
           ,' + '''' + c.[LegalEntityName] + '''' + '	   
		   ,' + '''' + CASE WHEN c.LegalEntityOrganisationType = 1 THEN 'CompaniesHouse' WHEN c.LegalEntityOrganisationType=2 THEN 'Charities' WHEN c.LegalEntityOrganisationType=3 THEN 'PublicBodies' ELSE 'Other' END + '''' + '
		   ,' + CASE WHEN a.StartDate is null then 'null' else + '''' + convert(varchar(10),a.StartDate,120) + '''' end + '
		   ,null
           ,' + CASE WHEN a.DateOfBirth is null then 'null' else + '''' + convert(varchar(10),a.DateOfBirth,120) + '''' end + '
		   ,' + CASE WHEN c.TransferSenderId is null then 'null' else + '''' + convert(varchar,c.TransferSenderId) + '''' end + '
		   ,' + CASE WHEN c.TransferSenderName is null then 'null' else + '''' + c.TransferSenderName + '''' end + '
		   ,' + CASE WHEN c.TransferApprovalStatus is null then 'null' else + '''' + convert(varchar,c.TransferApprovalStatus) + '''' end + '
		   ,' + CASE WHEN c.TransferApprovalActionedOn is null then 'null' else + '''' + convert(varchar(10),c.TransferApprovalActionedOn,120) + '''' end + '
		   ,' + CASE WHEN a.StopDate is null then 'null' else + '''' + convert(varchar(10),a.StopDate,120) + '''' end + '
		   ,' + CASE WHEN a.PauseDate is null then 'null' else + '''' + convert(varchar(10),a.PauseDate,120) + '''' end + '
		   ,' + CASE WHEN c.AccountLegalEntityPublicHashedId is null then 'null' else + '''' + convert(varchar,c.AccountLegalEntityPublicHashedId) + '''' end + '
		   );
		   '
		   from Apprenticeship a 
		   join Commitment c on c.Id = a.CommitmentId
		   where a.Id = @emitEventApprenticeshipId

		   set @outputScript = @outputScript + 'set @apprenticeshipEventsId = SCOPE_IDENTITY();'	   
		   
		   --Price History

		   select
		   @outputScript = @outputScript +
		   'INSERT INTO [dbo].[PriceHistory]
           ([ApprenticeshipEventsId]
           ,[TotalCost]
           ,[EffectiveFrom]
           ,[EffectiveTo])
			VALUES
           (
		   @apprenticeshipEventsId
		   ,' + convert(varchar,ph.Cost,120) + '
           ,' + convert(varchar(10),ph.FromDate,120) + '
		   ,' + CASE WHEN ph.ToDate is null then 'null' else + '''' + convert(varchar(10),ph.ToDate,120) + '''' end + '
		   )'
		   from PriceHistory ph
		   where ph.ApprenticeshipId = @emitEventApprenticeshipId


		   print @outputScript

	/* END LIFT 'N' SHIFT EMIT EVENT SCRIPT*/


	FETCH NEXT FROM apprenticeshipTargetsCursor INTO @emitEventApprenticeshipId
	END
	CLOSE apprenticeshipTargetsCursor
	DEALLOCATE apprenticeshipTargetsCursor


COMMIT
GO

ALTER TABLE Apprenticeship drop column CloneOf;
GO


