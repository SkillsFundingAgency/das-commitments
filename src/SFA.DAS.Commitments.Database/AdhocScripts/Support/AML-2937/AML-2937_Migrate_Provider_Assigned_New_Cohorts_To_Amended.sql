/*
Provider draft cohorts will, in future, be denoted as those having an EditStatus=2 and LastAction=0.
This pair of values currently denotes a cohort that has been sent straight to the provider during the employer
cohort creation journey.

This script updates the LastAction to 1 for cohorts sent straight to the provider. This will make no material difference,
since the cohorts are gathered into the same "For Review" bingo box anyway.

This will then free up this set of values for use later to denote Provider drafts.
*/


update Commitment
set LastAction = 1 --Set to 1 ("Amend")
where
EditStatus = 2 --Provider assigned 
and LastAction = 0 --None ("New")



