/*
Adhoc script to add PriceHistory records for 10 approved Apprenticeship records identified by Payments
as being in an invalid state.
*/

insert into PriceHistory
(ApprenticeshipId, Cost, FromDate, ToDate)
select
a.Id, a.Cost, a.StartDate, null
from Apprenticeship a
where a.Id in (132462,132360,665328,627949,665325,666932,95072,132361,665332,494495)
