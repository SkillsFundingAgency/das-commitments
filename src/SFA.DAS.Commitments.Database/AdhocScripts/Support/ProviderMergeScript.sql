/*
This script merges two Providers. It does so by stopping and backdating any apprenticeships that are active (or paused)
after the end of the academic year and creating an exact clone of the original except that it will be for the new provider.

Apprenticeships that are part of a Transfer are NOT currently covered by this script and will be ignored.

*/

--Parameters:

declare @oldProviderId bigint = 10005124; -- UKPRN of the discontinued provider
declare @newProviderId bigint = 10005124; -- UKPRN of the continuing provider 
declare @newProviderName nvarchar(100) = null; --The new name of the continuing provider, if applicable, or NULL if no change
declare @academicYearStartDate datetime = '2018-08-01'; --The date of the new academic year - apprenticeships before this date will not be affected

/* 
############################ DON'T GO CHANGING THINGS BELOW THIS LINE! ############################
*/

SET NOCOUNT ON


begin tran

--0. Get provider name
--1. Get the target apprenticeships
--2. Create a single commitment per employer account / legal entity combination
--3. Insert a new apprenticeship for each target, for the new UKPRN
--4. Stop and backdate the target apprenticeships

--0. Get providername
select top 1 @newProviderName = COALESCE(@newProviderName, ProviderName) from Commitment where ProviderId = @newProviderId order by id desc

print 'Merging apprenticeships for ' + convert(varchar,@oldProviderId) + ' into provider account ' + convert(varchar,@newProviderId) + ' ' + @newProviderName


--1. Get the target apprenticeships
declare @apprenticeshipTargets as table (ApprenticeshipId bigint)

insert into @apprenticeshipTargets
select Id as ApprenticeshipId
from
ApprenticeshipSummary
where ProviderId = @oldProviderId
and EndDate >= @academicYearStartDate
and (StopDate is null OR StopDate >= @academicYearStartDate)



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

select * from Commitment



rollback