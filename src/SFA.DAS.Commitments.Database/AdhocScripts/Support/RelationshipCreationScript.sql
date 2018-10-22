/*
Relationship creation script

The purpose of this script is to create a Employer-Provider Relationship record in commitments. Under normal circumstances,
a Relationship is created when the Employer first sends a Provider a Cohort. When this Relationship is not created due to
technical issues at the time (at time of writing, the two operations are not atomic), when the Provider goes to view the cohort
and exception is thrown and the only known work-around is to have the Employer send another, empty cohort, to their Provider.
*/

declare @ProviderId bigint = 0; -- Set the UKPRN of the provider for whom to create the relationship
declare @EmployerAccountId bigint = 0; -- Set the AccountId of the employer for whom to create the relationship
declare @LegalEntityCode nvarchar(50) = ''; --Set the Code of the legal entity for whom to create the relationship



/*
************* MODIFY BELOW THIS LINE AT YOUR PERIL *******************
*/

insert into [dbo].[Relationship]
select
top 1
c.ProviderId,
c.ProviderName,
c.EmployerAccountId,
c.LegalEntityId,
c.LegalEntityName,
c.LegalEntityAddress,
c.LegalEntityOrganisationType,
null as Verified,
GETDATE() as CreatedOn
from
[dbo].[Commitment] c
where c.ProviderId = @ProviderId and c.EmployerAccountId = @EmployerAccountId and c.LegalEntityId = @LegalEntityCode
and not exists (select 1 from Relationship where ProviderId = @ProviderId and EmployerAccountId = @EmployerAccountId and LegalEntityId = @LegalEntityCode)
order by c.Id desc


if(@@ROWCOUNT=0)
begin
	print 'Error - Relationship already exists, or no Commitment found'
end
