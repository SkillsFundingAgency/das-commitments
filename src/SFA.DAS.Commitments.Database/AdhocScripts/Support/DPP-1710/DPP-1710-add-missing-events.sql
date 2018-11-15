/*
This script emits an event for all apprenticeships for the specified ukprns.
It must be dealt with in two parts. First, this script must be run against the commitments db.
It generates a script that must then be executed against the events db.
*/

declare @oldProviderId bigint = 10000275; --the id of the provider where the apprenticeships are before the merge
declare @newProviderId bigint = 10000473; --the id of the provider where the apprenticeships should be moved to.

/* ========================================================================== */
/* =================== DO NOT MODIFY BELOW THIS LINE ======================== */
/* ========================================================================== */

set nocount on;

print 'declare @apprenticeshipEventsId bigint;'

declare @emitEventApprenticeshipId BIGINT;
declare cur cursor local for
with apprenticeshipsWithCloneOf as (
  select a.*
  from Apprenticeship a
  join Commitment c
	on c.Id = a.CommitmentId
  where a.CloneOf is not null
  and c.ProviderId in (@oldProviderId, @newProviderId)
), sourceApprenticeships as (
  select a.*
  from Apprenticeship a
  join apprenticeshipsWithCloneOf c
    on a.Id = c.CloneOf
), apprenticeshipsMissingMergeEvents as (
  select s.* from sourceApprenticeships s
  union
  select a.* from apprenticeshipsWithCloneOf a
)
select Id from apprenticeshipsMissingMergeEvents
open cur 
fetch next from cur into @emitEventApprenticeshipId

while @@FETCH_STATUS = 0 begin
  declare @outputScript as nvarchar(max);
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
    ,' + CASE WHEN a.[ULN] is null then 'null' else '''' + convert(varchar,a.[ULN]) + '''' end + '		   
    ,' + convert(varchar,c.[EmployerAccountId]) + '
    ,' + CASE WHEN a.[TrainingType] is null then 'null' WHEN a.TrainingType=0 THEN '1' ELSE '0' END + '		   
    ,' + CASE WHEN a.[TrainingCode] is null then 'null' else '''' + convert(varchar,a.[TrainingCode]) + '''' end + '		   
    ,' + CASE WHEN a.StartDate is null then 'null' else + '''' + convert(varchar(10),a.StartDate,120) + '''' end + '
    ,' + CASE WHEN a.EndDate is null then 'null' else + '''' + convert(varchar(10),a.EndDate,120) + '''' end + '
    ,' + CASE WHEN a.Cost is null then 'null' else convert(varchar,a.Cost) end + '		   
    ,' + '''' + convert(varchar,c.LegalEntityId,120) + '''' + '
    ,' + '''' + replace(c.[LegalEntityName],'''','''''') + '''' + '	   
    ,' + '''' + CASE WHEN c.LegalEntityOrganisationType = 1 THEN 'CompaniesHouse' WHEN c.LegalEntityOrganisationType=2 THEN 'Charities' WHEN c.LegalEntityOrganisationType=3 THEN 'PublicBodies' ELSE 'Other' END + '''' + '
    ,' + CASE WHEN a.StartDate is null then 'null' else + '''' + convert(varchar(10),a.StartDate,120) + '''' end + '
    ,null
    ,' + CASE WHEN a.DateOfBirth is null then 'null' else + '''' + convert(varchar(10),a.DateOfBirth,120) + '''' end + '
    ,' + CASE WHEN c.TransferSenderId is null then 'null' else + '''' + convert(varchar,c.TransferSenderId) + '''' end + '
    ,' + CASE WHEN c.TransferSenderName is null then 'null' else + '''' + replace(c.TransferSenderName,'''','''''') + '''' end + '
    ,' + CASE WHEN c.TransferApprovalStatus is null then 'null' else + '''' + convert(varchar,c.TransferApprovalStatus) + '''' end + '
    ,' + CASE WHEN c.TransferApprovalActionedOn is null then 'null' else + '''' + convert(varchar(10),c.TransferApprovalActionedOn,120) + '''' end + '
    ,' + CASE WHEN a.StopDate is null then 'null' else + '''' + convert(varchar(10),a.StopDate,120) + '''' end + '
    ,' + CASE WHEN a.PauseDate is null then 'null' else + '''' + convert(varchar(10),a.PauseDate,120) + '''' end + '
    ,' + CASE WHEN c.AccountLegalEntityPublicHashedId is null then 'null' else + '''' + convert(varchar,c.AccountLegalEntityPublicHashedId) + '''' end + '
    );'
  from Apprenticeship a 
  join Commitment c on c.Id = a.CommitmentId
  where a.Id = @emitEventApprenticeshipId
  
  set @outputScript = @outputScript + 'set @apprenticeshipEventsId = SCOPE_IDENTITY();'	   
  
 
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
    ,' + '''' + convert(varchar(10),ph.FromDate,120) + '''' + '
    ,' + CASE WHEN ph.ToDate is null then 'null' else + '''' + convert(varchar(10),ph.ToDate,120) + '''' end + '
    )'
  from PriceHistory ph
  where ph.ApprenticeshipId = @emitEventApprenticeshipId

  print @outputScript
  print '-- ================================================================================'
  fetch next from cur into @emitEventApprenticeshipId
end
close cur;
deallocate cur;

set nocount off;
