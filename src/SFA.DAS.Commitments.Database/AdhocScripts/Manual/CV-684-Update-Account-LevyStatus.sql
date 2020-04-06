/*
CV-684 - Initial set of Account levy status
Usage instructions:
Execute the below againsts the Accounts db.
Copy-paste the resulting output (a set of update statements), and execute those against the Commitments database.
*/

select
'update [Accounts] set [LevyStatus]=1 where [Id]=' + convert(varchar,Id)
from [employer_account].[Account]
where ApprenticeshipEmployerType = 1
