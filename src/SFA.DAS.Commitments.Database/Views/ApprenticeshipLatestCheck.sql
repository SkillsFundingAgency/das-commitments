CREATE VIEW [dbo].[ApprenticeshipLatestCheck]
	AS

-- the view determines the chronologically latest Apprenticeship record for all ULN & TrainingCode combinations
-- and set IsLatest = 1 or 0.
-- if there is no ULN or TrainingCode on the Apprenticeship record then IsLatest will be NULL.
-- In addiiton to IsLatest the sequence in reverse order is provided as LearningOrder (where 1 is also the latest)
-- and if an Apprenticeship record was Cancelled - IsCancelled = 1 or 0

WITH LatestApprenticeship
AS (
	SELECT Id, CASE WHEN LearningOrder = 1 THEN 1 ELSE 0 END IsLatest, LearningOrder
    FROM (
		SELECT id,
		  ROW_NUMBER() OVER (PARTITION BY ULN, TrainingCode ORDER BY 
				CASE 
				-- cancelled, not started, effectively deleted
				WHEN (StopDate IS NOT NULL AND EOMONTH(StopDate) = EOMONTH(StartDate) AND PaymentStatus = 3) THEN 10
				-- if StopDate of previous record is before Startdate of current record then prefer current record
				WHEN StopDate_1 IS NOT NULL AND EOMONTH(StartDate) >= EOMONTH(StopDate_1) THEN 0 
				-- if StopDate of current record is before Startdate of previous record then prefer previous record
				WHEN StopDate IS NOT NULL AND EOMONTH(StopDate) <= EOMONTH(StartDate_1) THEN 1 
				-- if both have StopDate then most recent one is the likely latest record
				WHEN StopDate IS NOT NULL AND StopDate_1 IS NOT NULL AND EOMONTH(StopDate_1) > EOMONTH(Stopdate) THEN 1 
				ELSE 0 END
				,CASE 
				WHEN ProviderId_1 != 0 AND ProviderId != ProviderId_1 THEN
				-- different provider, watch-out for retro-Approvals for earlier training period
					(CASE WHEN StartDate < StartDate_1 AND EndDate < Enddate_1 THEN 1 ELSE 0 END)
				ELSE
				-- same Provider or only one record, so use latest CreatedOn unless	
				-- ... possibly have same Provider with duplicated data (see PRB0041207 )
				-- Prefer the record with Active, Paused or Completed payment status, over Awaiting Approval or Cancelled
					(CASE PaymentStatus WHEN 0 THEN 1 /* Awaiting Approval */
										WHEN 1 THEN 0 /* Active */
										WHEN 2 THEN 0 /* Paused */
										WHEN 3 THEN 1 /* Cancelled */
										WHEN 4 THEN 0 /* Completed */ 
										ELSE 1 END)   /* N/A */
				END
				,CreatedOn DESC ) LearningOrder
			FROM (
			-- inner query gets all records for each Apprenticeship, ORDER BY CreatedOn desc - there can be many but two iterations should be sufficient
				SELECT ap1.Id
					,ap1.ULN
					,ap1.TrainingCode
					,ap1.StartDate
					,ap1.StopDate
					,ap1.EndDate
					,ap1.PaymentStatus
					,ap1.CreatedOn
				    ,cm1.ProviderId
					,LAG(ProviderId, 1,0) OVER (PARTITION BY ULN, TrainingCode ORDER BY ap1.CreatedOn ) AS ProviderId_1
					,LAG(StartDate, 1,0) OVER (PARTITION BY ULN, TrainingCode ORDER BY ap1.CreatedOn ) AS StartDate_1
					,LAG(EndDate, 1,0) OVER (PARTITION BY ULN, TrainingCode ORDER BY ap1.CreatedOn ) AS EndDate_1
					,LAG(CONVERT(datetime,StopDate), 1,0) OVER (PARTITION BY ULN, TrainingCode ORDER BY ap1.CreatedOn ) AS StopDate_1
				FROM Apprenticeship ap1
				JOIN Commitment cm1 on cm1.Id = ap1.CommitmentId
				WHERE ULN IS NOT NULL
					AND TrainingCode IS NOT NULL
				) OrderedApprenticeships
		) ApprenticeshipStatuses
)
SELECT Apprenticeship.*, IsLatest, LearningOrder,
       -- cancelled, not started, effectively deleted
	   CASE WHEN (StopDate IS NOT NULL AND EOMONTH(StopDate) = EOMONTH(StartDate) AND PaymentStatus = 3) THEN 1 ELSE 0 END IsCancelled  
FROM Apprenticeship 
LEFT JOIN LatestApprenticeship ON LatestApprenticeship.Id = Apprenticeship.Id

GO
