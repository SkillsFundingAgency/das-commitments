/*
Data script for DPP-1539: Consume & associate the Agreement ID against existing Commitments, Re-emit latest events part 1
Author: Phil Davies
Date: 24/07/2018
Target: EAS Employer_Account DB
Description: Generates script to insert (copy and paste) into part 2
*/

select 'insert into @sourcedata (EmployerAccountId, LegalEntityId, AccountLegalEntityPublicHashedId) values ('
+ convert(varchar, ale.AccountId) + ', ''' + le.Code + ''', ''' + ale.PublicHashedId + ''')'
from employer_account.AccountLegalEntity ale
inner join employer_account.LegalEntity le
on ale.LegalEntityId = le.Id