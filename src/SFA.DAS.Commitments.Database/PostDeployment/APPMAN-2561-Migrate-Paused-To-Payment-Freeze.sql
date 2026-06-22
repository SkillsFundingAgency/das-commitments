/*
APPMAN-2561 - Migrate legacy employer Paused apprenticeships to ILR payment freeze model.

For apprenticeships with PaymentStatus = Paused, a PauseDate, and no PaymentFreezeDate:
  - PaymentStatus -> Active (1)
  - PaymentFreezeDate -> PauseDate
  - FreezePaymentsReason -> 1 (Learner is on a break)
  - PauseDate -> NULL
*/

UPDATE Apprenticeship
SET PaymentStatus = 1,
    PaymentFreezeDate = PauseDate,
    FreezePaymentsReason = 1,
    PauseDate = NULL
WHERE PaymentStatus = 2 -- Paused
  AND PauseDate IS NOT NULL
  AND PaymentFreezeDate IS NULL;
