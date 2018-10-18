/*
Script to remove data locks with price error code if the user has also had a start date data lock
We do not have dlock 09 stored in commitments, so have to establish the existence of one by
inferring it where the effective from date in the ILR is before the course start date
If the user attempts to triage a data lock in this state, they receive an exception.
*/


update dl
set dl.IsExpired = 1
from Apprenticeship a
join DataLockStatus dl on dl.ApprenticeshipId = a.Id
where
[Status]=2 and (ErrorCode & 64 = 64) and IsResolved=0 and IsExpired=0 and EventStatus<>3
and dl.IlrEffectiveFromDate < a.StartDate


